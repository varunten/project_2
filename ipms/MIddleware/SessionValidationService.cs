using System.Security.Claims;
using IPMS.DAL.Data;
using IPMS.DTO.Entities;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Middlewares;


// For every authenticated request, checks that the session (token family) in
// the JWT is still valid (not revoked). Blocks logged-out / revoked sessions
// even while their short-lived access token is technically unexpired.
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, AppDbContext dbContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            string? sid = httpContext.User.FindFirst(ClaimTypes.Sid)?.Value;

            // No / malformed session id => treat as unauthenticated.
            if (!Guid.TryParse(sid, out Guid familyId))
            {
                httpContext.Response.StatusCode = 401;
                return;
            }

            TokenFamily? family = await dbContext.TokenFamilies
                .FirstOrDefaultAsync(tf => tf.Id == familyId);

            if (family is null || family.RevokedAt is not null)
            {
                httpContext.Response.StatusCode = 401;
                return;
            }
        }

        await _next(httpContext);
    }
}
