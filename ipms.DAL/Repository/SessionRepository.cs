using IPMS.DAL.Data;
using IPMS.DAL.IRepository;
using IPMS.DTO.Entities;
using Microsoft.EntityFrameworkCore;

namespace IPMS.DAL.Repository;


public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<List<TokenFamily>> GetActiveFamiliesByUserIdAsync(Guid userId)
    {
        return await _context.TokenFamilies
            .Where(tf => tf.UserId == userId && tf.RevokedAt == null)
            .OrderByDescending(tf => tf.CreatedAt)
            .ToListAsync();
    }


    public async Task<Guid?> GetFamilyIdByActiveTokenHashAsync(string tokenHash)
    {
        RefreshToken? token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.UsedAt == null);

        return token?.FamilyId;
    }


    public async Task<TokenFamily?> GetFamilyByIdForUserAsync(Guid familyId, Guid userId)
    {
        return await _context.TokenFamilies
            .FirstOrDefaultAsync(tf => tf.Id == familyId && tf.UserId == userId);
    }


    public async Task RevokeAllForUserAsync(Guid userId)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        await _context.TokenFamilies
            .Where(tf => tf.UserId == userId && tf.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(tf => tf.RevokedAt, now)
                .SetProperty(tf => tf.UpdatedAt, now));
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
