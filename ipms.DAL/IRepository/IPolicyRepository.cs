using IPMS.DTO.Entities;

namespace IPMS.DAL.IRepository;


public interface IPolicyRepository
{
    Task<List<Policy>> GetByCustomerIdAsync(Guid customerId);

    Task<Policy?> GetByIdAsync(Guid policyId);

    Task<Policy?> GetByIdForCustomerAsync(Guid policyId, Guid customerId);

    Task AddAsync(Policy policy);

    Task SaveChangesAsync();
}
