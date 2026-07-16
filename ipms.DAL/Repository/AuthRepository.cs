using IPMS.DAL.IRepository;
using IPMS.DTO.Entities;
using Microsoft.EntityFrameworkCore;
using IPMS.DAL.Data;

namespace IPMS.DAL.Repository;


public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;


    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(
                u => u.Email == email
            );
    }



    public async Task<bool> UserExistsAsync(
        string email,
        string phoneNumber)
    {
        return await _context.Users
            .AnyAsync(
                u =>
                u.Email == email ||
                u.PhoneNumber == phoneNumber
            );
    }



    public async Task<Role?> GetRoleByNameAsync(
        string name)
    {
        return await _context.Roles
            .SingleOrDefaultAsync(
                r => r.Name == name
            );
    }



    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }



    public async Task AddUserRoleAsync(UserRole userRole)
    {
        await _context.UserRoles.AddAsync(userRole);
    }



    public async Task AddTokenFamilyAsync(
        TokenFamily family)
    {
        await _context.TokenFamilies.AddAsync(family);
    }



    public async Task AddRefreshTokenAsync(
        RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
    }



    public async Task<RefreshToken?> GetRefreshTokenAsync(
        string tokenHash)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(
                rt => rt.TokenHash == tokenHash
            );
    }



    public async Task<TokenFamily?> GetTokenFamilyAsync(
        Guid familyId)
    {
        return await _context.TokenFamilies
            .FirstOrDefaultAsync(
                tf => tf.Id == familyId
            );
    }



    public async Task<User?> GetUserByIdAsync(
        Guid userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == userId
            );
    }



    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        return await (
            from userRole in _context.UserRoles
            join role in _context.Roles
                on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name
        ).ToListAsync();
    }



    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Where(u => u.DeletedAt == null)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }



    public async Task<Dictionary<Guid, List<string>>> GetRolesForUsersAsync(
        List<Guid> userIds)
    {
        var rows = await (
            from userRole in _context.UserRoles
            join role in _context.Roles
                on userRole.RoleId equals role.Id
            where userIds.Contains(userRole.UserId)
            select new { userRole.UserId, role.Name }
        ).ToListAsync();

        return rows
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Name).ToList());
    }



    public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId)
    {
        return await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }



    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task RevokeRefreshTokensAsync(Guid familyId)
{
    await _context.RefreshTokens
        .Where(rt =>
            rt.FamilyId == familyId &&
            rt.UsedAt == null
        )
        .ExecuteUpdateAsync(
            setters => setters
                .SetProperty(
                    rt => rt.UsedAt,
                    DateTimeOffset.UtcNow
                )
                .SetProperty(
                    rt => rt.UpdatedAt,
                    DateTimeOffset.UtcNow
                )
        );
}
}