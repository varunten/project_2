using System.Security.Claims;
using IPMS.BAL.IService;
using IPMS.DTO;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClaimController : ControllerBase
{
    private readonly IClaimService _service;

    public ClaimController(IClaimService service)
    {
        _service = service;
    }


    // Customer files a claim.
    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ApiResponse<ClaimDto>>> CreateClaim(CreateClaimDto payload)
    {
        ClaimDto result = await _service.CreateClaimAsync(GetUserId(), payload);
        return Ok(ApiResponse.Ok(result, "Claim filed."));
    }


    // A customer's own claims. Literal "my" must sit above "{claim_id}".
    [HttpGet("my")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ApiResponse<ClaimsDto>>> GetMyClaims()
    {
        ClaimsDto result = await _service.GetMyClaimsAsync(GetUserId());
        return Ok(ApiResponse.Ok(result, "Claims retrieved."));
    }


    // Staff review the claim queue.
    [HttpGet]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<ApiResponse<ClaimsDto>>> GetClaims()
    {
        ClaimsDto result = await _service.GetClaimsAsync();
        return Ok(ApiResponse.Ok(result, "Claims retrieved."));
    }


    [HttpGet("{claim_id}")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<ApiResponse<ClaimDto>>> GetClaimById(Guid claim_id)
    {
        ClaimDto result = await _service.GetClaimByIdAsync(claim_id);
        return Ok(ApiResponse.Ok(result, "Claim retrieved."));
    }


    // Underwriter reviews / settles.
    [HttpPatch("{claim_id}")]
    [Authorize(Roles = Roles.Underwriter)]
    public async Task<ActionResult<ApiResponse<ClaimDto>>> UpdateClaim(
        Guid claim_id,
        UpdateClaimDto payload)
    {
        ClaimDto result = await _service.UpdateClaimAsync(GetUserId(), claim_id, payload);
        return Ok(ApiResponse.Ok(result, "Claim updated."));
    }


    [HttpPost("{claim_id}/documents")]
    public async Task<ActionResult<ApiResponse<ClaimDocumentDto>>> UploadDocument(
        Guid claim_id,
        UploadClaimDocumentDto payload)
    {
        ClaimDocumentDto result = await _service.UploadDocumentAsync(GetUserId(), claim_id, payload);
        return Ok(ApiResponse.Ok(result, "Document uploaded."));
    }


    [HttpGet("{claim_id}/documents")]
    public async Task<ActionResult<ApiResponse<List<ClaimDocumentDto>>>> GetClaimDocuments(Guid claim_id)
    {
        List<ClaimDocumentDto> result = await _service.GetClaimDocumentsAsync(claim_id);
        return Ok(ApiResponse.Ok(result, "Documents retrieved."));
    }


    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
