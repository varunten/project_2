using ipms.MVC.Services;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ipms.MVC.Controllers;


// The customer's own profile: create it once, then view and edit it.
public class CustomerController : BaseController
{
    private readonly IpmsApiClient _api;

    public CustomerController(IpmsApiClient api)
    {
        _api = api;
    }


    // "My Profile": show the profile if it exists, otherwise send them to Create.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            CustomerDto profile = await _api.GetMyProfileAsync();
            return View(profile);
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == 401)
                return RedirectToAction("Login", "Account");

            // 404 = no profile yet -> let them create one.
            if (ex.StatusCode == 404)
                return RedirectToAction(nameof(Create));

            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Products");
        }
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

        if (!ModelState.IsValid) return View(payload);

        try
        {
            await _api.CreateCustomerAsync(payload);

            TempData["Success"] = "Profile created.";
            // Land on the profile so they can see it, not vanish to products.
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            AddApiErrors(ex);
            return View(payload);
        }
    }


    [HttpPost]
    public async Task<IActionResult> Edit(UpdateCustomerDto payload)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        try
        {
            await _api.UpdateMyProfileAsync(payload);

            TempData["Success"] = "Profile updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
