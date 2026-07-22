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


    // Loaded into the "view claim" modal. Shows the claim + its documents.
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        if (!IsLoggedIn)
            return Content("Your session has expired. Please sign in again.");

        try
        {
            // GetMyClaim verifies the claim belongs to this customer; only then
            // do we load its documents.
            ClaimDto claim = await _api.GetMyClaimAsync(id);
            List<ClaimDocumentDto> documents = await _api.GetClaimDocumentsAsync(id);

            ViewBag.Documents = documents;
            return PartialView("_ClaimDetails", claim);
        }
        catch (ApiException ex)
        {
            return Content(ex.Message);
        }
    }


    // Customer adds a document (metadata only - no file upload) to their claim.
    [HttpPost]
    public async Task<IActionResult> AddDocument(Guid claimId, UploadClaimDocumentDto payload)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            // Confirm ownership before attaching anything to the claim.
            await _api.GetMyClaimAsync(claimId);
            await _api.UploadClaimDocumentAsync(claimId, payload);

            TempData["Success"] = "Document added to the claim.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
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
