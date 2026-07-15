using IPMS.DAL.Data;
using IPMS.DAL.IRepository;
using IPMS.DTO.Entities;
using Microsoft.EntityFrameworkCore;

namespace IPMS.DAL.Repository;


public class PolicyRepository : IPolicyRepository
{
    private readonly AppDbContext _context;

    public PolicyRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<List<Policy>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Policies
            .Where(p => p.CustomerId == customerId && p.DeletedAt == null)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }


    public async Task<Policy?> GetByIdAsync(Guid policyId)
    {
        return await _context.Policies
            .FirstOrDefaultAsync(p => p.Id == policyId && p.DeletedAt == null);
    }


    public async Task<Policy?> GetByIdForCustomerAsync(Guid policyId, Guid customerId)
    {
        return await _context.Policies
            .FirstOrDefaultAsync(p =>
                p.Id == policyId &&
                p.CustomerId == customerId &&
                p.DeletedAt == null);
    }


    public async Task AddAsync(Policy policy)
    {
        await _context.Policies.AddAsync(policy);
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
