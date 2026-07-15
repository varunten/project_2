using System.Security.Claims;
using IPMS.BAL.IService;
using IPMS.DTO;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Customer)]
public class PolicyController : ControllerBase
{
    private readonly IPolicyService _service;

    public PolicyController(IPolicyService service)
    {
        _service = service;
    }


    [HttpGet]
    public async Task<ActionResult<ApiResponse<PoliciesDto>>> GetPolicies()
    {
        PoliciesDto result = await _service.GetPoliciesAsync(GetUserId());
        return Ok(ApiResponse.Ok(result, "Policies retrieved."));
    }


    [HttpGet("{policy_id}")]
    public async Task<ActionResult<ApiResponse<PolicyDto>>> GetPolicyById(Guid policy_id)
    {
        PolicyDto result = await _service.GetPolicyByIdAsync(GetUserId(), policy_id);
        return Ok(ApiResponse.Ok(result, "Policy retrieved."));
    }


    [HttpPatch("{policy_id}/cancel")]
    public async Task<ActionResult<ApiResponse<PolicyDto>>> CancelPolicy(
        Guid policy_id,
        CancelPolicyDto payload)
    {
        PolicyDto result = await _service.CancelPolicyAsync(GetUserId(), policy_id, payload);
        return Ok(ApiResponse.Ok(result, "Policy cancelled."));
    }


    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
