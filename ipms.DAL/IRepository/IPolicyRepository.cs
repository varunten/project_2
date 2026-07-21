using IPMS.DTO.Entities;

namespace IPMS.DAL.IRepository;


public interface IPolicyRepository
{
    Task<List<Policy>> GetByCustomerIdAsync(Guid customerId);

    Task<Policy?> GetByIdAsync(Guid policyId);

    Task<Policy?> GetByIdForCustomerAsync(Guid policyId, Guid customerId);

    // True when a renewal policy already points back at this one, so the same
    // policy cannot be renewed twice.
    Task<bool> HasRenewalAsync(Guid previousPolicyId);

    Task AddAsync(Policy policy);

    Task SaveChangesAsync();
}
