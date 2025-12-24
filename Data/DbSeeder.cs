using System.Security.Claims;
using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Authorization.Handlers;
using T3mmyvsa.Entities;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            // 1. Seed Roles
            foreach (var role in Enum.GetValues<AppRole>())
            {
                var roleName = role.ToString();
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Default Admin User
            var adminEmail = "admin@oyoehaulage.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "P@ssword123!");
                await userManager.AddToRoleAsync(adminUser, AppRole.Admin.ToString());
            }

            // 3. Seed Permissions for Admin Role
            var adminRoleName = AppRole.Admin.ToString();
            var adminRole = await roleManager.FindByNameAsync(adminRoleName);
            if (adminRole != null)
            {
                var existingClaims = await roleManager.GetClaimsAsync(adminRole);

                foreach (var permission in Enum.GetValues<AppPermission>())
                {
                    var permissionValue = permission.GetDescription();
                    if (!existingClaims.Any(c => c.Type == PermissionAuthorizationHandler.PermissionClaimType && c.Value == permissionValue))
                    {
                        await roleManager.AddClaimAsync(adminRole, new Claim(PermissionAuthorizationHandler.PermissionClaimType, permissionValue));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't stop the application
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
