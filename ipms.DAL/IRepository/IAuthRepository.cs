using IPMS.DTO.Entities;


namespace IPMS.DAL.IRepository;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email);

    Task<bool> UserExistsAsync(
        string email,
        string phoneNumber
    );

    Task<Role?> GetRoleByNameAsync(string name);

    Task AddUserAsync(User user);

    Task AddUserRoleAsync(UserRole userRole);

    Task AddTokenFamilyAsync(TokenFamily family);

    Task AddRefreshTokenAsync(RefreshToken refreshToken);

    Task<RefreshToken?> GetRefreshTokenAsync(
        string tokenHash
    );

    Task<TokenFamily?> GetTokenFamilyAsync(
        Guid familyId
    );

    Task<User?> GetUserByIdAsync(
        Guid userId
    );

    // Role names assigned to a user (put into the JWT so [Authorize(Roles)] works).
    Task<List<string>> GetUserRolesAsync(Guid userId);

    // Whether a user already has a given role (avoids duplicate assignments).
    Task<bool> UserHasRoleAsync(Guid userId, Guid roleId);

    // Marks every still-usable refresh token in a family as used, so a whole
    // session can be invalidated at once (used on refresh-token reuse).
    Task RevokeRefreshTokensAsync(Guid familyId);

    Task SaveChangesAsync();
}