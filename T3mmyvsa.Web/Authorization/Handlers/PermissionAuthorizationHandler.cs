using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using T3mmyvsa.Authorization.Requirements;
using T3mmyvsa.Interfaces;

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
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IUserPermissionService>();
        var permissions = await permissionService.GetPermissionsAsync(userId);

        var authorized = requirement.RequireAll
            ? requirement.RequiredPermissions.All(permissions.Contains)
            : requirement.RequiredPermissions.Any(permissions.Contains);

        if (authorized)
        {
            context.Succeed(requirement);
        }
    }
}
