using System.Security.Claims;
using IPMS.DAL.Data;

namespace IPMS;


// Reads the current user's id from the JWT on the active HTTP request, so the
// DbContext can stamp it onto audit rows. Returns null when there is no signed-in
// user (anonymous request, background work, design-time tooling).
public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetUserId()
    {
        string? value = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out Guid userId) ? userId : null;
    }
}
