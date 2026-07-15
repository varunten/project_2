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
}
