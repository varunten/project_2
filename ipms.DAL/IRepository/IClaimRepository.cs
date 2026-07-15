using IPMS.DTO.Entities;

namespace IPMS.DAL.IRepository;


public interface IClaimRepository
{
    Task<List<Claim>> GetAllAsync();

    // Claims filed against any of the given policies (a customer's own claims).
    Task<List<Claim>> GetByPolicyIdsAsync(List<Guid> policyIds);

    Task<Claim?> GetByIdAsync(Guid claimId);

    Task AddAsync(Claim claim);

    Task<List<ClaimDocument>> GetDocumentsByClaimIdAsync(Guid claimId);

    Task AddDocumentAsync(ClaimDocument document);

    Task SaveChangesAsync();
}
