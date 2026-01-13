using System.Security.Claims;
using FruitHub.ApplicationCore.Models;
using FruitHub.Infrastructure.Identity.Models;
using FruitHub.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace FruitHub.Infrastructure.Identity.Seeders;

public static class IdentitySeeder
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        var config = services.GetRequiredService<IConfiguration>();
        
        var adminFullName = config["ADMIN:FULLNAME"];
        var adminUserName = config["ADMIN:USERNAME"];
        var adminEmail    = config["ADMIN:EMAIL"];
        var adminPassword = config["ADMIN:PASSWORD"];

        const string adminRole = "Admin";

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
