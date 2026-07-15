using IPMS.DTO.Dtos;

namespace IPMS.BAL.IService;


public interface IClaimService
{
    // Customer files a claim against one of their policies.
    Task<ClaimDto> CreateClaimAsync(Guid userId, CreateClaimDto payload);

    // The signed-in customer's own claims (across all their policies).
    Task<ClaimsDto> GetMyClaimsAsync(Guid userId);

    // Every claim - the staff review queue.
    Task<ClaimsDto> GetClaimsAsync();

    Task<ClaimDto> GetClaimByIdAsync(Guid claimId);

    // Underwriter reviews / settles the claim.
    Task<ClaimDto> UpdateClaimAsync(Guid underwriterId, Guid claimId, UpdateClaimDto payload);

    Task<ClaimDocumentDto> UploadDocumentAsync(Guid userId, Guid claimId, UploadClaimDocumentDto payload);

    Task<List<ClaimDocumentDto>> GetClaimDocumentsAsync(Guid claimId);
}
