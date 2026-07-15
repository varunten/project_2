using ipms.MVC.Services;
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
}
