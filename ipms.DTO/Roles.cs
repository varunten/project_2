namespace IPMS.DTO;


// Role name constants. These match the names seeded by RoleSeeder and the
// role claims put into the JWT at login, so [Authorize(Roles = ...)] works.
public static class Roles
{
    public const string Customer = "Customer";
    public const string Admin = "Admin";
    public const string InsuranceAgent = "InsuranceAgent";
    public const string Underwriter = "Underwriter";

    // Handy grouping for endpoints that any staff member may use.
    public const string Staff = Admin + "," + InsuranceAgent + "," + Underwriter;
}
