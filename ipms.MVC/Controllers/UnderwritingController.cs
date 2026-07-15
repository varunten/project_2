using ipms.MVC.Services;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ipms.MVC.Controllers;


// The underwriter's screen: quotes customers have accepted, waiting for a
// decision. Approving one issues the policy.
public class UnderwritingController : BaseController
{
    private readonly IpmsApiClient _api;

    public UnderwritingController(IpmsApiClient api)
    {
        _api = api;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            QuotesDto pending = await _api.GetPendingQuotesAsync();
            return View(pending);
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == 401)
                return RedirectToAction("Login", "Account");

            TempData["Error"] = ex.Message;
            return View(new QuotesDto());
        }
    }


    [HttpPost]
    public async Task<IActionResult> Approve(Guid id)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            PolicyDto policy = await _api.ApproveQuoteAsync(id);
            TempData["Success"] = $"Quote approved. Policy {policy.PolicyNumber} issued.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    public async Task<IActionResult> Reject(Guid id)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            await _api.RejectQuoteAsync(id);
            TempData["Success"] = "Quote rejected.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
