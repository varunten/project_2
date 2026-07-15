using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ipms.MVC.Services;


// Reads the roles out of the access token so the UI can show the right menu
// links. This is only for display - the API is what actually enforces roles.
public static class JwtHelper
{
    public static List<string> GetRoles(string accessToken)
    {
        try
        {
            JwtSecurityToken token = new JwtSecurityTokenHandler()
                .ReadJwtToken(accessToken);

            return token.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList();
        }
        catch (Exception)
        {
            // A token we can't read just means "no roles" - the API will still
            // reject anything this user isn't allowed to do.
            return [];
        }
    }
}
