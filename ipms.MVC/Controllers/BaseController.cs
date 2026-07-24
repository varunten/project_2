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


    // Most customer pages are useless until the user has created their customer
    // profile, and the API rejects them with a plain message. The API has no
    // error code to key off, so we match on the status + wording it uses. The
    // 409 "already exists" case deliberately falls outside this.
    protected static bool IsProfileMissing(ApiException ex) =>
        (ex.StatusCode == 400 || ex.StatusCode == 404) &&
        ex.Message.Contains("customer profile", StringComparison.OrdinalIgnoreCase);


    // Puts an API error where the layout can show it. A missing customer
    // profile becomes a friendly message plus a link to the page that fixes it,
    // instead of a dead end telling the user what they cannot do.
    protected void SetApiError(ApiException ex)
    {
        if (IsProfileMissing(ex))
        {
            SetProfileRequiredError();
            return;
        }

        TempData["Error"] = ex.Message;
    }


    protected void SetProfileRequiredError()
    {
        TempData["Error"] = "You need to update the customer profile before you can use this.";
        TempData["ErrorLinkText"] = "Update customer profile";
        TempData["ErrorLinkController"] = "Customer";
        TempData["ErrorLinkAction"] = "Index";
    }


    // Turns an API error into form errors: a 422's per-field messages land on
    // the matching input, anything else becomes one message at the top.
    protected void AddApiErrors(ApiException ex)
    {
        if (ex.Errors is { Count: > 0 })
        {
            foreach ((string field, string[] messages) in ex.Errors)
            {
                string key = NormalizeFieldKey(field);
                foreach (string message in messages)
                {
                    ModelState.AddModelError(key, message);
                }
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
    }


    // The API may report a field as "Email", "email" or "$.email"; the form's
    // <span asp-validation-for="Email"> expects "Email". Line them up.
    private static string NormalizeFieldKey(string field)
    {
        string key = field.StartsWith("$.") ? field[2..] : field;

        if (key.Length > 0 && char.IsLower(key[0]))
            key = char.ToUpper(key[0]) + key[1..];

        return key;
    }
}
