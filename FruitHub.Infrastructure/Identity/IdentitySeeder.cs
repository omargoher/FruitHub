using System.Security.Claims;
using FruitHub.ApplicationCore.Models;
using FruitHub.Infrastructure.Persistence;

namespace FruitHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

public static class IdentitySeeder
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        const string adminFullName = "Omar Goher";
        const string adminUserName = "OmarGoher";
        const string adminRole = "Admin";
        const string adminEmail = "admin@fruithub.com";
        const string adminPassword = "Admin@123"; // move to config later

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, adminPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, adminRole))
        {
            await userManager.AddToRoleAsync(user, adminRole);
        }

        // Add business entity  
        var adminExists = dbContext.Set<Admin>()
            .Any(a => a.UserId == user.Id);

        if (!adminExists)
        {
            var admin = new Admin
            {
                UserId = user.Id,
                Email = adminEmail,
                FullName = adminFullName,
            };
            
            dbContext.Add(admin);

            await dbContext.SaveChangesAsync();
            
            await userManager.AddClaimAsync(user, new Claim(
                "business_admin_id",
                admin.Id.ToString()
            ));
        }
    }
}
