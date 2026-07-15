using IPMS.DAL.Data;
using IPMS.DAL.IRepository;
using IPMS.DTO.Entities;
using Microsoft.EntityFrameworkCore;

namespace IPMS.DAL.Repository;


public class PremiumPaymentRepository : IPremiumPaymentRepository
{
    private readonly AppDbContext _context;

    public PremiumPaymentRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<List<PremiumPayment>> GetByPolicyIdAsync(Guid policyId)
    {
        return await _context.PremiumPayments
            .Where(p => p.PolicyId == policyId)
            .OrderBy(p => p.DueDate)
            .ToListAsync();
    }


    public async Task<PremiumPayment?> GetByIdAsync(Guid paymentId)
    {
        return await _context.PremiumPayments
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }


    public async Task AddAsync(PremiumPayment payment)
    {
        await _context.PremiumPayments.AddAsync(payment);
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
