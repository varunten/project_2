using ipms.MVC.Services;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ipms.MVC.Controllers;


public class PoliciesController : BaseController
{
    private readonly IpmsApiClient _api;

    public PoliciesController(IpmsApiClient api)
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
            PoliciesDto policies = await _api.GetPoliciesAsync();
            return View(policies);
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == 401)
                return RedirectToAction("Login", "Account");

            TempData["Error"] = ex.Message;
            return View(new PoliciesDto { Total = 0, Policies = [] });
        }
    }


    [HttpPost]
    public async Task<IActionResult> Renew(Guid id)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            PolicyDto renewed = await _api.RenewPolicyAsync(id);
            TempData["Success"] = $"Policy renewed. New policy {renewed.PolicyNumber} " +
                                  $"runs to {renewed.EndDate:yyyy-MM-dd}.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }


    // Loaded into the "view policy" modal on the list page. Returns just the
    // fragment (no layout) so it can be dropped straight into the modal body.
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        if (!IsLoggedIn)
            return Content("Your session has expired. Please sign in again.");

        try
        {
            PolicyDto policy = await _api.GetPolicyAsync(id);
            return PartialView("_PolicyDetails", policy);
        }
        catch (ApiException ex)
        {
            return Content(ex.Message);
        }
    }
}
