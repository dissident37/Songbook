using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Songbook.Web.Models;

namespace Songbook.Web.Auth;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Admin-Rolle anlegen (falls nicht vorhanden)
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!roleResult.Succeeded)
                throw new Exception("Admin-Rolle konnte nicht erstellt werden.");
        }

        // Admin-User anlegen (nur für Entwicklung / Demo)
        var adminEmail = "admin@songbook.local";
        var adminPassword = "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new Exception($"Admin-User konnte nicht erstellt werden: {errors}");
            }
        }

        // Admin-Rolle zuweisen (falls noch nicht gesetzt)
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!addRoleResult.Succeeded)
                throw new Exception("Admin-Rolle konnte nicht zugewiesen werden.");
        }
    }
}
