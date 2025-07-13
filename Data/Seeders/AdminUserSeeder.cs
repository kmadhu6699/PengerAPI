using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PengerAPI.Models;
using System;
using System.Threading.Tasks;

namespace PengerAPI.Data.Seeders
{
    public static class AdminUserSeeder
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Create Admin Role if it doesn't exist
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Create default admin user if it doesn't exist
                var adminEmail = configuration["AdminUser:Email"] ?? "admin@penger.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    var adminUsername = configuration["AdminUser:Username"] ?? "admin";
                    var adminPassword = configuration["AdminUser:Password"] ?? "Admin@123";

                    var newAdmin = new ApplicationUser
                    {
                        UserName = adminUsername,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "User",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(newAdmin, adminPassword);
                    
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newAdmin, "Admin");
                    }
                }
            }
        }
    }
}
