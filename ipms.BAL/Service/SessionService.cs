using System.Security.Cryptography;
using System.Text;
using IPMS.BAL.IService;
using IPMS.DAL.IRepository;
using IPMS.DTO.Dtos;
using IPMS.DTO.Entities;

namespace IPMS.BAL.Service;


public class SessionService : ISessionService
{
    private readonly ISessionRepository _repository;

    public SessionService(ISessionRepository repository)
    {
        _repository = repository;
    }


    public async Task<SessionsDto> GetSessionsAsync(Guid userId, string refreshToken)
    {
        string tokenHash = HashToken(refreshToken);

        Guid? currentFamilyId = await _repository.GetFamilyIdByActiveTokenHashAsync(tokenHash);

        List<TokenFamily> families = await _repository.GetActiveFamiliesByUserIdAsync(userId);

        SessionsDto result = new();

        foreach (TokenFamily family in families)
        {
            result.Sessions.Add(new SessionDto
            {
                FamilyId = family.Id,
                CreatedAt = family.CreatedAt,
                Current = family.Id == currentFamilyId
            });
        }

        return result;
    }


    public async Task RevokeAllSessionsAsync(Guid userId)
    {
        await _repository.RevokeAllForUserAsync(userId);
    }


    public async Task RevokeSessionAsync(Guid userId, Guid familyId)
    {
        TokenFamily? family = await _repository.GetFamilyByIdForUserAsync(familyId, userId);

        // Silently succeed if it doesn't exist / already revoked (idempotent).
        if (family is null || family.RevokedAt is not null)
            return;

        family.RevokedAt = DateTimeOffset.UtcNow;

        await _repository.SaveChangesAsync();
    }


    private static string HashToken(string token)
    {
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
