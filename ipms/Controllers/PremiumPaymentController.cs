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
public class PremiumPaymentController : ControllerBase
{
    private readonly IPremiumPaymentService _service;

    public PremiumPaymentController(IPremiumPaymentService service)
    {
        _service = service;
    }


    [HttpGet("policy/{policy_id}")]
    public async Task<ActionResult<ApiResponse<PremiumPaymentsDto>>> GetPolicyPayments(Guid policy_id)
    {
        bool isStaff =
            User.IsInRole(Roles.Admin) ||
            User.IsInRole(Roles.InsuranceAgent) ||
            User.IsInRole(Roles.Underwriter);

        PremiumPaymentsDto result =
            await _service.GetPolicyPaymentsAsync(GetUserId(), policy_id, isStaff);

        return Ok(ApiResponse.Ok(result, "Payments retrieved."));
    }


    // Staff generates installments for a policy.
    [HttpPost]
    [Authorize(Roles = Roles.Admin + "," + Roles.InsuranceAgent)]
    public async Task<ActionResult<ApiResponse<PremiumPaymentDto>>> CreatePremiumPayment(
        CreatePremiumPaymentDto payload)
    {
        PremiumPaymentDto result = await _service.CreatePremiumPaymentAsync(payload);
        return Ok(ApiResponse.Ok(result, "Premium installment created."));
    }


    // Customer pays one of their installments.
    [HttpPost("{payment_id}/pay")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ApiResponse<PremiumPaymentDto>>> PayPremium(
        Guid payment_id,
        PayPremiumDto payload)
    {
        PremiumPaymentDto result = await _service.PayPremiumAsync(GetUserId(), payment_id, payload);
        return Ok(ApiResponse.Ok(result, "Premium paid."));
    }


    [HttpPatch("{payment_id}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.InsuranceAgent)]
    public async Task<ActionResult<ApiResponse<PremiumPaymentDto>>> UpdatePremiumPayment(
        Guid payment_id,
        UpdatePremiumPaymentDto payload)
    {
        PremiumPaymentDto result = await _service.UpdatePremiumPaymentAsync(payment_id, payload);
        return Ok(ApiResponse.Ok(result, "Premium payment updated."));
    }


    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
