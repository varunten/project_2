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
    public async Task<IActionResult> Index([FromQuery] ProductQueryDto query)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        // Echo the current filter/sort back to the view so the controls stay in
        // sync and the pager can rebuild links that keep the same filters.
        ViewBag.Query = query;

        try
        {
            ProductsDto products = await _api.GetProductsAsync(query);
            return View(products);
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == 401)
                return RedirectToAction("Login", "Account");

            SetApiError(ex);
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

        if (!ModelState.IsValid) return View(payload);

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
