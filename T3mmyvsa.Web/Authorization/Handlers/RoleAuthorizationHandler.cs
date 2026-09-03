using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using T3mmyvsa.Authorization.Requirements;
using T3mmyvsa.Entities;

namespace T3mmyvsa.Authorization.Handlers;

public class RoleAuthorizationHandler(IServiceScopeFactory serviceScopeFactory) : AuthorizationHandler<RoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
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
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return;
        }

        var roles = await userManager.GetRolesAsync(user);
        var authorized = requirement.RequireAll
            ? requirement.AllowedRoles.All(role => roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            : requirement.AllowedRoles.Any(role => roles.Contains(role, StringComparer.OrdinalIgnoreCase));

        if (authorized)
        {
            context.Succeed(requirement);
        }
    }
}
