using ipms.MVC.Services;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ipms.MVC.Controllers;


public class QuotesController : BaseController
{
    private readonly IpmsApiClient _api;

    public QuotesController(IpmsApiClient api)
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
            QuotesDto quotes = await _api.GetQuotesAsync();
            return View(quotes);
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == 401)
                return RedirectToAction("Login", "Account");

            TempData["Error"] = ex.Message;
            return View(new QuotesDto());
        }
    }


    // Reached from the "Request a quote" button on the products page.
    [HttpGet]
    public async Task<IActionResult> Create(Guid productId)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            ProductDto product = await _api.GetProductAsync(productId);

            ViewBag.ProductName = product.Name;
            ViewBag.DefaultCoverage = product.CoverageAmount;

            return View(new CreateQuoteDto
            {
                ProductId = product.Id,
                CoverageAmount = product.CoverageAmount
            });
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == 401)
                return RedirectToAction("Login", "Account");

            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Products");
        }
    }


    [HttpPost]
    public async Task<IActionResult> Create(CreateQuoteDto payload)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            await _api.CreateQuoteAsync(payload);

            TempData["Success"] = "Quote requested.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            AddApiErrors(ex);
            return View(payload);
        }
    }


    [HttpPost]
    public async Task<IActionResult> Accept(Guid id)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            await _api.AcceptQuoteAsync(id);
            TempData["Success"] = "Quote accepted. It now goes to an underwriter for approval.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
