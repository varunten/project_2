using ipms.MVC.Services;
using IPMS.DTO;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ipms.MVC.Controllers;


public class ProductsController : BaseController
{
    private readonly IpmsApiClient _api;

    public ProductsController(IpmsApiClient api)
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
            ProductsDto products = await _api.GetProductsAsync();
            return View(products);
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == 401)
                return RedirectToAction("Login", "Account");

            TempData["Error"] = ex.Message;
            return View(new ProductsDto { Total = 0, Products = [] });
        }
    }


    // Insurance agents own the catalogue - they create the products.

    [HttpGet]
    public IActionResult Create()
    {
        IActionResult? denied = RequireAgent();
        if (denied is not null) return denied;

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto payload)
    {
        IActionResult? denied = RequireAgent();
        if (denied is not null) return denied;

        try
        {
            ProductDto product = await _api.CreateProductAsync(payload);

            TempData["Success"] = $"Product '{product.Name}' created.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            AddApiErrors(ex);
            return View(payload);
        }
    }


    // Only insurance agents may manage products. The API enforces this too.
    private IActionResult? RequireAgent()
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        if (!IsInRole(Roles.InsuranceAgent))
        {
            TempData["Error"] = "Only insurance agents can manage products.";
            return RedirectToAction(nameof(Index));
        }

        return null;
    }
}
