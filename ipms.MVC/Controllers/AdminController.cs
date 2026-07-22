using ipms.MVC.Services;
using IPMS.DTO;
using IPMS.DTO.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ipms.MVC.Controllers;


// Admin-only screens: manage users, staff accounts and the product catalogue.
public class AdminController : BaseController
{
    private readonly IpmsApiClient _api;

    public AdminController(IpmsApiClient api)
    {
        _api = api;
    }


    // Blocks non-admins from every action on this controller. The API enforces
    // this too - this just keeps the UI honest.
    private IActionResult? RequireAdmin()
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        if (!IsInRole(Roles.Admin))
        {
            TempData["Error"] = "Admins only.";
            return RedirectToAction("Index", "Home");
        }

        return null;
    }


    // ---- Users ----

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        IActionResult? denied = RequireAdmin();
        if (denied is not null) return denied;

        try
        {
            UsersDto users = await _api.GetUsersAsync();
            return View(users);
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View(new UsersDto { Total = 0, Users = [] });
        }
    }


    [HttpPost]
    public async Task<IActionResult> AssignRole(Guid id, string role)
    {
        IActionResult? denied = RequireAdmin();
        if (denied is not null) return denied;

        try
        {
            List<string> roles = await _api.AssignRoleAsync(id, new AssignRoleDto { Role = role });
            TempData["Success"] = $"Role added. User now has: {string.Join(", ", roles)}.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Users));
    }


    // ---- Staff ----

    [HttpGet]
    public IActionResult CreateStaff()
    {
        IActionResult? denied = RequireAdmin();
        if (denied is not null) return denied;

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> CreateStaff(CreateStaffDto payload)
    {
        IActionResult? denied = RequireAdmin();
        if (denied is not null) return denied;

        if (!ModelState.IsValid) return View(payload);

        try
        {
            UserDto staff = await _api.CreateStaffAsync(payload);

            TempData["Success"] = $"Staff account created for {staff.Email} ({payload.Role}).";
            return RedirectToAction(nameof(Users));
        }
        catch (ApiException ex)
        {
            AddApiErrors(ex);
            return View(payload);
        }
    }
}
