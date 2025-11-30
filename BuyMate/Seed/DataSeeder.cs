using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuyMate.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = services.GetRequiredService<UserManager<BuyMate.Model.Entities.User>>();
            var config = services.GetRequiredService<IConfiguration>();

            await EnsureRoleExists(roleManager, "admin");
            await EnsureRoleExists(roleManager, "user");

            await EnsureAdminUserAsync(userManager, config);
        }

        private static async Task EnsureRoleExists(RoleManager<IdentityRole<Guid>> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole<Guid>(roleName);
                var result = await roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors);
                    throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
                }
            }
        }

        private static async Task EnsureAdminUserAsync(UserManager<BuyMate.Model.Entities.User> userManager, IConfiguration config)
        {
            var email = config["SeedAdmin:Email"];
            var password = config["SeedAdmin:Password"];
            var phone = config["SeedAdmin:Phone"] ?? string.Empty;
            var firstName = config["SeedAdmin:FirstName"] ?? "Admin";
            var lastName = config["SeedAdmin:LastName"] ?? "User";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return; // Do not seed without secure configured credentials
            }

            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                if (!await userManager.IsInRoleAsync(existing, "admin"))
                {
                    await userManager.AddToRoleAsync(existing, "admin");
                }
                return;
            }

            var user = new BuyMate.Model.Entities.User
            {
                UserName = BuildUserNameFromEmail(email),
                Email = email,
                PhoneNumber = phone,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(phone)
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                var errors = string.Join("; ", create.Errors);
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(user, "admin");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors);
                throw new InvalidOperationException($"Admin user created but role assignment failed: {errors}");
            }
        }

        private static string BuildUserNameFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return Guid.NewGuid().ToString();
            var parts = email.Split('@');
            return parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : Guid.NewGuid().ToString();
        }
    }
}
