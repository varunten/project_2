using IPMS.DAL.Data;
using IPMS.DAL.IRepository;
using IPMS.DTO.Entities;
using Microsoft.EntityFrameworkCore;

namespace IPMS.DAL.Repository;


public class ClaimRepository : IClaimRepository
{
    private readonly AppDbContext _context;

    public ClaimRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<List<Claim>> GetAllAsync()
    {
        return await _context.Claims
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }


    public async Task<List<Claim>> GetByPolicyIdsAsync(List<Guid> policyIds)
    {
        return await _context.Claims
            .Where(c => policyIds.Contains(c.PolicyId))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }


    public async Task<Claim?> GetByIdAsync(Guid claimId)
    {
        return await _context.Claims
            .FirstOrDefaultAsync(c => c.Id == claimId);
    }


    public async Task AddAsync(Claim claim)
    {
        await _context.Claims.AddAsync(claim);
    }


    public async Task<List<ClaimDocument>> GetDocumentsByClaimIdAsync(Guid claimId)
    {
        return await _context.ClaimDocuments
            .Where(d => d.ClaimId == claimId)
            .OrderBy(d => d.UploadedAt)
            .ToListAsync();
    }


    public async Task AddDocumentAsync(ClaimDocument document)
    {
        await _context.ClaimDocuments.AddAsync(document);
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
