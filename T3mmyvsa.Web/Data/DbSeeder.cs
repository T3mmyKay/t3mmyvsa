using System.Security.Claims;
using Microsoft.Extensions.Options;
using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Authorization.Handlers;
using T3mmyvsa.Configuration;
using T3mmyvsa.Entities;
using T3mmyvsa.Extensions;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var userRoleService = serviceProvider.GetRequiredService<IUserRoleService>();
        var bootstrapAdmin = serviceProvider.GetRequiredService<IOptions<BootstrapAdminSettings>>().Value;

        foreach (var role in Enum.GetValues<AppRole>())
        {
            var roleName = role.ToString();
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var createRoleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!createRoleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", createRoleResult.Errors.Select(x => x.Description)));
            }
        }

        var adminRoleName = AppRole.Admin.ToString();
        var adminRole = await roleManager.FindByNameAsync(adminRoleName)
            ?? throw new InvalidOperationException("The Admin role could not be loaded after seeding.");
        var existingClaims = await roleManager.GetClaimsAsync(adminRole);

        foreach (var permission in Enum.GetValues<AppPermission>())
        {
            var permissionValue = permission.GetDescription();
            if (existingClaims.Any(c =>
                    c.Type == PermissionAuthorizationHandler.PermissionClaimType && c.Value == permissionValue))
            {
                continue;
            }

            var claimResult = await roleManager.AddClaimAsync(
                adminRole,
                new Claim(PermissionAuthorizationHandler.PermissionClaimType, permissionValue));
            if (!claimResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", claimResult.Errors.Select(x => x.Description)));
            }
        }

        if (!bootstrapAdmin.Enabled)
        {
            logger.LogInformation("Bootstrap admin provisioning is disabled.");
            return;
        }

        var adminEmail = bootstrapAdmin.Email!.Trim();
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = bootstrapAdmin.FirstName.Trim(),
                LastName = bootstrapAdmin.LastName.Trim(),
                EmailConfirmed = true,
                IsActive = true,
                LockoutEnabled = true
            };

            var createUserResult = await userManager.CreateAsync(adminUser, bootstrapAdmin.Password!);
            if (!createUserResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", createUserResult.Errors.Select(x => x.Description)));
            }
        }

        await userRoleService.SetExactRoleAsync(adminUser, adminRoleName);
        logger.LogInformation("Bootstrap admin provisioning completed for configured account {AdminUserId}.", adminUser.Id);
    }
}
