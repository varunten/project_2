using IPMS.DTO.Dtos;

namespace IPMS.BAL.IService;


public interface IPolicyService
{
    Task<PoliciesDto> GetPoliciesAsync(Guid userId);

    Task<PolicyDto> GetPolicyByIdAsync(Guid userId, Guid policyId);

    Task<PolicyDto> CancelPolicyAsync(Guid userId, Guid policyId, CancelPolicyDto payload);

    // Issues a new policy that continues on from an existing one.
    Task<PolicyDto> RenewPolicyAsync(Guid userId, Guid policyId);
}
