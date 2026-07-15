using System.Net.Http.Headers;

namespace ipms.MVC.Services;


// Puts "Authorization: Bearer <token>" on every request to the API, reading the
// token from the signed-in user's session. This means the controllers never
// have to think about the token.
public class AuthTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? token = _httpContextAccessor.HttpContext?
            .Session.GetString(SessionKeys.AccessToken);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
