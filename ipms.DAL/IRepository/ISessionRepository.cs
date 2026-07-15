using IPMS.DTO.Entities;

namespace IPMS.DAL.IRepository;


public interface ISessionRepository
{
    Task<List<TokenFamily>> GetActiveFamiliesByUserIdAsync(Guid userId);

    // Which family owns the currently-usable refresh token with this hash (if any).
    Task<Guid?> GetFamilyIdByActiveTokenHashAsync(string tokenHash);

    Task<TokenFamily?> GetFamilyByIdForUserAsync(Guid familyId, Guid userId);

    Task RevokeAllForUserAsync(Guid userId);

    Task SaveChangesAsync();
}
