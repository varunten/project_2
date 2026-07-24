using ipms.MVC.Services;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ipms.MVC.Controllers;


public class AccountController : BaseController
{
    private readonly IpmsApiClient _api;

    public AccountController(IpmsApiClient api)
    {
        _api = api;
    }


    [HttpGet]
    public IActionResult Signup()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Signup(AuthSignupDto payload)
    {
        // Show field errors (from the DTO's data annotations) straight away,
        // under each box, without a round trip to the API.
        if (!ModelState.IsValid)
            return View(payload);

        try
        {
            await _api.SignupAsync(payload);

            TempData["Success"] = "Account created. Please sign in.";
            return RedirectToAction(nameof(Login));
        }
        catch (ApiException ex)
        {
            AddApiErrors(ex);
            return View(payload);
        }
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Login(AuthLoginDto payload)
    {
        if (!ModelState.IsValid)
            return View(payload);

        try
        {
            TokenDto token = await _api.LoginAsync(payload);

            HttpContext.Session.SetString(SessionKeys.AccessToken, token.AccessToken);
            HttpContext.Session.SetString(SessionKeys.RefreshToken, token.RefreshToken);
            // The API stores emails lowercased, so show it the same way here
            // rather than echoing back whatever casing was typed.
            HttpContext.Session.SetString(
                SessionKeys.Email, payload.Email.Trim().ToLowerInvariant());

            // Remember the roles so the menu can show the right links.
            List<string> roles = JwtHelper.GetRoles(token.AccessToken);
            HttpContext.Session.SetString(SessionKeys.Roles, string.Join(",", roles));

            // Send each role to the page they actually need.
            if (roles.Contains(IPMS.DTO.Roles.Admin))
                return RedirectToAction("Users", "Admin");

            if (roles.Contains(IPMS.DTO.Roles.Underwriter))
                return RedirectToAction("Index", "Underwriting");

            return RedirectToAction("Index", "Products");
        }
        catch (ApiException ex)
        {
            AddApiErrors(ex);
            return View(payload);
        }
    }


    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        TempData["Success"] = "You have been signed out.";
        return RedirectToAction("Index", "Home");
    }
}
