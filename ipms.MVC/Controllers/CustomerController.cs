using ipms.MVC.Services;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ipms.MVC.Controllers;


// A signed-in user must create a customer profile before they can ask for
// quotes (the API rejects quote requests without one).
public class CustomerController : BaseController
{
    private readonly IpmsApiClient _api;

    public CustomerController(IpmsApiClient api)
    {
        _api = api;
    }


    [HttpGet]
    public IActionResult Create()
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerDto payload)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            await _api.CreateCustomerAsync(payload);

            TempData["Success"] = "Profile created. You can now request quotes.";
            return RedirectToAction("Index", "Products");
        }
        catch (ApiException ex)
        {
            AddApiErrors(ex);
            return View(payload);
        }
    }
}
