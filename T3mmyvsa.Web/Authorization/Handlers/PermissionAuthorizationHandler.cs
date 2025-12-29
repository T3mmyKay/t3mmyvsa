using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using T3mmyvsa.Authorization.Requirements;
using T3mmyvsa.Entities;

namespace T3mmyvsa.Authorization.Handlers;

public class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory) : AuthorizationHandler<PermissionRequirement>
{
    public const string PermissionClaimType = "Permission";

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return;
        }

        var userRoleNames = await userManager.GetRolesAsync(user);
        var userPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in userRoleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await roleManager.GetClaimsAsync(role);
                foreach (var claim in roleClaims.Where(c => c.Type == PermissionClaimType))
                {
                    userPermissions.Add(claim.Value);
                }
            }
        }

        // Check if user has all required permissions
        if (requirement.RequiredPermissions.All(p => userPermissions.Contains(p)))
        {
            context.Succeed(requirement);
        }
    }
}
