using IPMS.DTO.Entities;

namespace IPMS.DAL.IRepository;


public interface IPremiumPaymentRepository
{
    Task<List<PremiumPayment>> GetByPolicyIdAsync(Guid policyId);

    Task<PremiumPayment?> GetByIdAsync(Guid paymentId);

    Task AddAsync(PremiumPayment payment);

    Task SaveChangesAsync();
}
