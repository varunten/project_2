using IPMS.DTO.Dtos;

namespace IPMS.BAL.IService;


public interface IAuthService
{
    Task<UserDto> RegisterAsync(
        AuthSignupDto payload
    );


    Task<TokenDto> LoginAsync(
        AuthLoginDto payload
    );


    Task<TokenDto> RefreshAsync(
        RefreshTokenDto payload
    );


    Task LogoutAsync(
        RefreshTokenDto payload
    );


    Task<UserDto> CreateStaffAsync(
        CreateStaffDto payload
    );


    // Admin listing of every user with the roles they hold.
    Task<UsersDto> GetAllUsersAsync();


    // Gives an existing user another role; returns the user's roles afterwards.
    Task<List<string>> AssignRoleAsync(
        Guid userId,
        string role
    );
}