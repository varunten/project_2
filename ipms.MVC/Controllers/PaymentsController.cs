using ipms.MVC.Services;
using IPMS.DTO.Dtos;
using IPMS.DTO.Enum;
using Microsoft.AspNetCore.Mvc;

namespace ipms.MVC.Controllers;


// The premium schedule for one policy, and paying an installment.
public class PaymentsController : BaseController
{
    private readonly IpmsApiClient _api;

    public PaymentsController(IpmsApiClient api)
    {
        _api = api;
    }


    [HttpGet]
    public async Task<IActionResult> Index(Guid policyId)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            PremiumPaymentsDto payments = await _api.GetPolicyPaymentsAsync(policyId);

            ViewBag.PolicyId = policyId;
            return View(payments);
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == 401)
                return RedirectToAction("Login", "Account");

            SetApiError(ex);
            return RedirectToAction("Index", "Policies");
        }
    }


    [HttpPost]
    public async Task<IActionResult> Pay(Guid id, Guid policyId, decimal amount)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            PremiumPaymentDto paid = await _api.PayPremiumAsync(id, new PayPremiumDto
            {
                AmountPaid = amount,
                PaymentMethod = PremiumPaymentMethod.Bank
            });

            TempData["Success"] = $"Installment {paid.InstallmentNumber} paid ({paid.PaymentStatus}).";
        }
        catch (ApiException ex)
        {
            SetApiError(ex);
        }

        return RedirectToAction(nameof(Index), new { policyId });
    }
}
