using ipms.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace ipms.MVC.Controllers;


// Shared helpers for the pages that talk to the API.
public abstract class BaseController : Controller
{
    protected bool IsLoggedIn =>
        !string.IsNullOrEmpty(
            HttpContext.Session.GetString(SessionKeys.AccessToken));


    // Display-level check only. The API is what actually enforces roles.
    protected bool IsInRole(string role)
    {
        string roles = HttpContext.Session.GetString(SessionKeys.Roles) ?? "";
        return roles.Split(',').Contains(role);
    }


    // Turns an API error into form errors: a 422's per-field messages land on
    // the matching input, anything else becomes one message at the top.
    protected void AddApiErrors(ApiException ex)
    {
        if (ex.Errors is { Count: > 0 })
        {
            foreach ((string field, string[] messages) in ex.Errors)
            {
                foreach (string message in messages)
                {
                    ModelState.AddModelError(field, message);
                }
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
    }
}
