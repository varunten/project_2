using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IPMS.BAL.IService;
using IPMS.DAL.IRepository;
using IPMS.DTO;
using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;
using IPMS.DTO.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IPMS.BAL.Service;


public class AuthService : IAuthService
{

    private readonly IAuthRepository _repository;
    private readonly IConfiguration _config;

    private readonly PasswordHasher<User> _PasswordHasher = new();

    private readonly string DummyPasswordHash;

    

    public AuthService(
    IAuthRepository repository,
    IConfiguration config)
{
    _repository = repository;
    _config = config;

    DummyPasswordHash =
        _PasswordHasher.HashPassword(
            null!,
            "correct horse battery staple"
        );
}



    public async Task<UserDto> RegisterAsync(
        AuthSignupDto payload)
    {

        if(payload.Password != payload.PasswordConfirm)
            throw new ValidationException(
                "Password and confirmation do not match."
            );


        bool exists =
            await _repository.UserExistsAsync(
                payload.Email,
                payload.PhoneNumber
            );


        if(exists)
            throw new ConflictException(
                "A user with this email or phone number already exists."
            );



        User user = new()
        {
            FirstName = payload.FirstName,
            MiddleName = payload.MiddleName,
            LastName = payload.LastName,
            Email = payload.Email,
            PhoneNumber = payload.PhoneNumber,
            PasswordHash =
                _PasswordHasher.HashPassword(
                    null!,
                    payload.Password
                )
        };


        await _repository.AddUserAsync(user);

        await _repository.SaveChangesAsync();



        Role role =
            await _repository.GetRoleByNameAsync(
                "Customer"
            )
            ?? throw new Exception(
                "Customer role missing"
            );


        await _repository.AddUserRoleAsync(
            new UserRole
            {
                UserId=user.Id,
                RoleId=role.Id
            }
        );


        await _repository.SaveChangesAsync();



        return ToUserDto(user);

    }

    public async Task<TokenDto> LoginAsync(AuthLoginDto payload)
{
    User? user = await _repository.GetUserByEmailAsync(payload.Email);

    if (user is null)
    {
        _PasswordHasher.VerifyHashedPassword(
            null!,
            DummyPasswordHash,
            payload.Password
        );

        throw new UnauthorizedException(
            "Invalid email or password"
        );
    }


    PasswordVerificationResult result =
        _PasswordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            payload.Password
        );


    if(result == PasswordVerificationResult.Failed)
    {
        throw new UnauthorizedException(
            "Invalid email or password"
        );
    }


    TokenFamily family = new()
    {
        UserId = user.Id
    };


    await _repository.AddTokenFamilyAsync(family);
    await _repository.SaveChangesAsync();



    string refreshTokenRaw =
        Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64)
        );


    string refreshTokenHash = HashToken(refreshTokenRaw);


    RefreshToken refreshToken = new()
    {
        TokenHash = refreshTokenHash,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        FamilyId = family.Id
    };


    await _repository.AddRefreshTokenAsync(refreshToken);
    await _repository.SaveChangesAsync();


    List<string> roles = await _repository.GetUserRolesAsync(user.Id);


    return new TokenDto
    {
        AccessToken = CreateToken(user, family.Id, roles),
        RefreshToken = refreshTokenRaw
    };
}

public async Task<TokenDto> RefreshAsync(
    RefreshTokenDto payload)
{
    string refreshTokenHash = HashToken(payload.Token);


    RefreshToken? refreshToken =
        await _repository.GetRefreshTokenAsync(
            refreshTokenHash
        );


    if(refreshToken is null)
    {
        throw new UnauthorizedException(
            "Invalid refresh token"
        );
    }


    if(refreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
    {
        throw new UnauthorizedException(
            "Refresh token expired"
        );
    }



    TokenFamily? family =
        await _repository.GetTokenFamilyAsync(
            refreshToken.FamilyId
        );


    if(family is null)
    {
        throw new UnauthorizedException(
            "Invalid token family"
        );
    }



    if(family.RevokedAt is not null)
    {
        throw new UnauthorizedException(
            "Session expired"
        );
    }



    if(refreshToken.UsedAt is not null)
    {
        family.RevokedAt =
            DateTimeOffset.UtcNow;


        // Reuse detected: kill the whole family's tokens, not just this one.
        await _repository.RevokeRefreshTokensAsync(
            family.Id
        );


        await _repository.SaveChangesAsync();


        throw new UnauthorizedException(
            "Refresh token reuse detected"
        );
    }



    refreshToken.UsedAt =
        DateTimeOffset.UtcNow;


    refreshToken.UpdatedAt =
        DateTimeOffset.UtcNow;



    User? user =
        await _repository.GetUserByIdAsync(
            family.UserId
        );


    if(user is null)
    {
        throw new UnauthorizedException(
            "User not found"
        );
    }



    string newRefreshTokenRaw =
        Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64)
        );


    string newRefreshTokenHash = HashToken(newRefreshTokenRaw);



    RefreshToken newRefreshToken = new()
    {
        TokenHash = newRefreshTokenHash,
        FamilyId = family.Id,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
    };


    await _repository.AddRefreshTokenAsync(
        newRefreshToken
    );


    await _repository.SaveChangesAsync();


    List<string> roles = await _repository.GetUserRolesAsync(user.Id);



    return new TokenDto
    {
        AccessToken = CreateToken(
            user,
            family.Id,
            roles
        ),

        RefreshToken = newRefreshTokenRaw
    };
}

public async Task LogoutAsync(
    RefreshTokenDto payload)
{

    string refreshTokenHash = HashToken(payload.Token);


    RefreshToken? refreshToken =
        await _repository.GetRefreshTokenAsync(
            refreshTokenHash
        );


    if(refreshToken is null)
    {
        return;
    }



    TokenFamily? family =
        await _repository.GetTokenFamilyAsync(
            refreshToken.FamilyId
        );


    if(family is null)
    {
        return;
    }



    refreshToken.UsedAt =
        DateTimeOffset.UtcNow;


    refreshToken.UpdatedAt =
        DateTimeOffset.UtcNow;



    family.RevokedAt =
        DateTimeOffset.UtcNow;


    family.UpdatedAt =
        DateTimeOffset.UtcNow;



    await _repository.SaveChangesAsync();
}

private string CreateToken(User user, Guid familyId, List<string> roles)
{
    List<System.Security.Claims.Claim> claims =
    [
        new System.Security.Claims.Claim(
            ClaimTypes.NameIdentifier,
            user.Id.ToString()
        ),

        new System.Security.Claims.Claim(
            ClaimTypes.Name,
            user.Email
        ),

        new System.Security.Claims.Claim(
            ClaimTypes.Sid,
            familyId.ToString()
        )
    ];


    // One role claim per role so [Authorize(Roles = ...)] can check them.
    foreach (string role in roles)
    {
        claims.Add(new System.Security.Claims.Claim(ClaimTypes.Role, role));
    }


    var key =
        new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _config.GetValue<string>(
                    "AppSettings:Token"
                )!
            )
        );


    var creds =
        new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha512
        );


    var token =
        new JwtSecurityToken(
            issuer:
                _config.GetValue<string>(
                    "AppSettings:Issuer"
                ),

            audience:
                _config.GetValue<string>(
                    "AppSettings:Audience"
                ),

            claims: claims,

            expires:
                DateTime.UtcNow.AddDays(1),

            signingCredentials: creds
        );


    return new JwtSecurityTokenHandler()
        .WriteToken(token);
}


public async Task<UserDto> CreateStaffAsync(CreateStaffDto payload)
{
    string[] allowedRoles =
    [
        Roles.Admin,
        Roles.InsuranceAgent,
        Roles.Underwriter
    ];

    if (!allowedRoles.Contains(payload.Role))
        throw new ValidationException(
            "Role must be one of: Admin, InsuranceAgent, Underwriter."
        );


    bool exists = await _repository.UserExistsAsync(
        payload.Email,
        payload.PhoneNumber
    );

    if (exists)
        throw new ConflictException(
            "A user with this email or phone number already exists."
        );


    User user = new()
    {
        FirstName = payload.FirstName,
        MiddleName = payload.MiddleName,
        LastName = payload.LastName,
        Email = payload.Email,
        PhoneNumber = payload.PhoneNumber,
        PasswordHash = _PasswordHasher.HashPassword(null!, payload.Password)
    };

    await _repository.AddUserAsync(user);
    await _repository.SaveChangesAsync();


    Role role = await _repository.GetRoleByNameAsync(payload.Role)
        ?? throw new Exception($"Role '{payload.Role}' is missing.");

    await _repository.AddUserRoleAsync(new UserRole
    {
        UserId = user.Id,
        RoleId = role.Id
    });

    await _repository.SaveChangesAsync();


    return ToUserDto(user);
}


public async Task<UsersDto> GetAllUsersAsync()
{
    List<User> users = await _repository.GetAllUsersAsync();

    Dictionary<Guid, List<string>> rolesByUser =
        await _repository.GetRolesForUsersAsync(users.Select(u => u.Id).ToList());

    List<UserWithRolesDto> dtos = users
        .Select(u => new UserWithRolesDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            MiddleName = u.MiddleName,
            LastName = u.LastName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Roles = rolesByUser.GetValueOrDefault(u.Id, []),
            CreatedAt = u.CreatedAt
        })
        .ToList();

    return new UsersDto
    {
        Total = (ulong)dtos.Count,
        Users = dtos
    };
}


public async Task<List<string>> AssignRoleAsync(Guid userId, string role)
{
    string[] allowedRoles =
    [
        Roles.Admin,
        Roles.InsuranceAgent,
        Roles.Underwriter,
        Roles.Customer
    ];

    if (!allowedRoles.Contains(role))
        throw new ValidationException(
            "Role must be one of: Admin, InsuranceAgent, Underwriter, Customer."
        );


    User user = await _repository.GetUserByIdAsync(userId)
        ?? throw new NotFoundException("User not found.");


    Role roleEntity = await _repository.GetRoleByNameAsync(role)
        ?? throw new Exception($"Role '{role}' is missing.");


    bool alreadyHasRole = await _repository.UserHasRoleAsync(user.Id, roleEntity.Id);

    if (alreadyHasRole)
        throw new ConflictException("User already has this role.");


    await _repository.AddUserRoleAsync(new UserRole
    {
        UserId = user.Id,
        RoleId = roleEntity.Id
    });

    await _repository.SaveChangesAsync();


    // The user must log in again (or refresh) to get a token with the new role.
    return await _repository.GetUserRolesAsync(user.Id);
}


// Refresh tokens are stored only as a SHA-256 hash, never in plain text.
private static string HashToken(string token)
{
    return Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}


private static UserDto ToUserDto(User user)
{
    return new UserDto
    {
        FirstName = user.FirstName,
        MiddleName = user.MiddleName,
        LastName = user.LastName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}

}