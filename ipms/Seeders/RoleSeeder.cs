using IPMS.DAL.Data;
using IPMS.DTO;
using IPMS.DTO.Entities;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Seeders;


public static class RoleSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        string[] roles =
        [
            Roles.Customer,
            Roles.Admin,
            Roles.InsuranceAgent,
            Roles.Underwriter
        ];

        foreach (string roleName in roles)
        {
            bool exists = await context.Roles
                .AnyAsync(r => r.Name == roleName);

            if (!exists)
            {
                context.Roles.Add(new Role { Name = roleName });
            }
        }

        await context.SaveChangesAsync();
    }
}
