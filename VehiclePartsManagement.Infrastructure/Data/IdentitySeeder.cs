using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data;

/// <summary>
/// Seeds baseline identity roles and users required for local development and testing.
/// </summary>
public static class IdentitySeeder
{
    /// <summary>
    /// Ensures default roles and users exist.
    /// </summary>
    /// <param name="services">Scoped service provider.</param>
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = ["Admin", "Staff", "Customer"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var usersToSeed = new[]
        {
            new { Email = "admin@gmail.com", FullName = "Admin User", Role = "Admin" },
            new { Email = "staff@gmail.com", FullName = "Staff User", Role = "Staff" },
            new { Email = "awantikapokharel46@gmail.com", FullName = "Awantika Customer", Role = "Customer" }
        };

        // Update existing test user email if it hasn't been updated yet
        var oldUser = await userManager.FindByEmailAsync("customer@gmail.com");
        if (oldUser != null)
        {
            oldUser.Email = "awantikapokharel46@gmail.com";
            oldUser.UserName = "awantikapokharel46@gmail.com";
            oldUser.NormalizedEmail = "AWANTIKAPOKHAREL46@GMAIL.COM";
            oldUser.NormalizedUserName = "AWANTIKAPOKHAREL46@GMAIL.COM";
            await userManager.UpdateAsync(oldUser);
        }

        foreach (var seedUser in usersToSeed)
        {
            // Idempotent seeding: create only when missing, then ensure role assignment.
            var user = await userManager.FindByEmailAsync(seedUser.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = seedUser.Email,
                    Email = seedUser.Email,
                    FullName = seedUser.FullName,
                    PhoneNumber = string.Empty,
                    Address = string.Empty,
                    RegisteredAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, "Hello@123");
                if (!createResult.Succeeded)
                {
                    continue;
                }
            }

            if (!await userManager.IsInRoleAsync(user, seedUser.Role))
            {
                await userManager.AddToRoleAsync(user, seedUser.Role);
            }
        }
    }
}
