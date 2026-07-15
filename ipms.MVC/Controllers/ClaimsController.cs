using ipms.MVC.Services;
using IPMS.DTO.Dtos;
using IPMS.DTO.Enum;
using Microsoft.AspNetCore.Mvc;

namespace ipms.MVC.Controllers;


public class ClaimsController : BaseController
{
    private readonly IpmsApiClient _api;

    public ClaimsController(IpmsApiClient api)
    {
        _api = api;
    }


    // ---- Customer ----

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            ClaimsDto claims = await _api.GetMyClaimsAsync();
            return View(claims);
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == 401)
                return RedirectToAction("Login", "Account");

            TempData["Error"] = ex.Message;
            return View(new ClaimsDto { Total = 0, Claims = [] });
        }
    }


    [HttpGet]
    public IActionResult Create(Guid policyId)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        return View(new CreateClaimDto
        {
            PolicyId = policyId,
            IncidentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ClaimAmount = 0
        });
    }


    [HttpPost]
    public async Task<IActionResult> Create(CreateClaimDto payload)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            await _api.CreateClaimAsync(payload);

            TempData["Success"] = "Claim filed. An underwriter will review it.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            AddApiErrors(ex);
            return View(payload);
        }
    }


    // ---- Underwriter ----

    [HttpGet]
    public async Task<IActionResult> Review()
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            ClaimsDto claims = await _api.GetAllClaimsAsync();
            return View(claims);
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == 401)
                return RedirectToAction("Login", "Account");

            TempData["Error"] = ex.Message;
            return View(new ClaimsDto { Total = 0, Claims = [] });
        }
    }


    [HttpPost]
    public async Task<IActionResult> Decide(Guid id, ClaimStatus status, string? notes)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            ClaimDto claim = await _api.UpdateClaimAsync(id, new UpdateClaimDto
            {
                Status = status,
                Notes = notes
            });

            TempData["Success"] = $"Claim {claim.ClaimNumber} is now {claim.Status}.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Review));
    }
}
