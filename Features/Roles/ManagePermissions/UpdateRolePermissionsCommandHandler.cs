using System.Security.Claims;
using T3mmyvsa.Authorization.Handlers;

namespace T3mmyvsa.Features.Roles.ManagePermissions;

public class UpdateRolePermissionsCommandHandler(RoleManager<IdentityRole> roleManager)
    : ICommandHandler<UpdateRolePermissionsCommand>
{
    public async Task Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.RoleId) ?? throw new KeyNotFoundException("Role not found.");
        var currentClaims = await roleManager.GetClaimsAsync(role);
        var currentPermissions = currentClaims
            .Where(c => c.Type == PermissionAuthorizationHandler.PermissionClaimType)
            .ToList();

        // Remove permissions that are not in the request
        foreach (var claim in currentPermissions)
        {
            if (!request.Permissions.Contains(claim.Value))
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }
        }

        // Add new permissions
        var existingPermissionValues = currentPermissions.Select(c => c.Value).ToHashSet();
        foreach (var permission in request.Permissions)
        {
            if (!existingPermissionValues.Contains(permission))
            {
                await roleManager.AddClaimAsync(role, new Claim(PermissionAuthorizationHandler.PermissionClaimType, permission));
            }
        }
    }
}
