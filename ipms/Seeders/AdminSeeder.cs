using IPMS.DAL.Data;
using IPMS.DTO;
using IPMS.DTO.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Seeders;

// Creates a single bootstrap Admin account on first run so there is at least
// one user who can create the other staff (agents / underwriters) via the API.
// Change the password after first login in a real deployment.
public static class AdminSeeder
{
    public const string DefaultAdminEmail = "admin@ipms.local";
    public const string DefaultAdminPassword = "Admin@123";

    public static async Task SeedAsync(AppDbContext context)
    {
        bool adminExists = await context.Users
            .AnyAsync(u => u.Email == DefaultAdminEmail);

        if (adminExists)
            return;

        PasswordHasher<User> hasher = new();

        User admin = new()
        {
            FirstName = "System",
            LastName = "Admin",
            Email = DefaultAdminEmail,
            PhoneNumber = "0000000000",
            PasswordHash = hasher.HashPassword(null!, DefaultAdminPassword)
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();

        Role adminRole = await context.Roles.FirstAsync(r => r.Name == Roles.Admin);

        context.UserRoles.Add(new UserRole
        {
            UserId = admin.Id,
            RoleId = adminRole.Id
        });

        await context.SaveChangesAsync();
    }
}
