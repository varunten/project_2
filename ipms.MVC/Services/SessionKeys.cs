namespace ipms.MVC.Services;


// Names of the values we keep in the user's session after they log in.
public static class SessionKeys
{
    public const string AccessToken = "access_token";
    public const string RefreshToken = "refresh_token";
    public const string Email = "email";
    public const string Roles = "roles";
}
