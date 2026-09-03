using System.Security.Claims;
using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Authorization.Handlers;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Roles.ManagePermissions;

public class UpdateRolePermissionsCommandHandler(RoleManager<IdentityRole> roleManager)
    : ICommandHandler<UpdateRolePermissionsCommand>
{
    public async Task Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.RoleId)
            ?? throw new KeyNotFoundException("Role not found.");

        if (request.Permissions.Count != request.Permissions.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            throw new InvalidOperationException("Permission values must be unique.");
        }

        var knownPermissions = Enum.GetValues<AppPermission>()
            .Select(x => x.GetDescription())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unknown = request.Permissions.Where(x => !knownPermissions.Contains(x)).ToArray();
        if (unknown.Length > 0)
        {
            throw new InvalidOperationException($"Unknown permission(s): {string.Join(", ", unknown)}");
        }

        var currentClaims = await roleManager.GetClaimsAsync(role);
        var currentPermissions = currentClaims
            .Where(x => x.Type == PermissionAuthorizationHandler.PermissionClaimType)
            .ToList();

        foreach (var claim in currentPermissions.Where(x => !request.Permissions.Contains(x.Value, StringComparer.OrdinalIgnoreCase)))
        {
            var remove = await roleManager.RemoveClaimAsync(role, claim);
            if (!remove.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", remove.Errors.Select(x => x.Description)));
            }
        }

        var existing = currentPermissions.Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var permission in request.Permissions.Where(x => !existing.Contains(x)))
        {
            var add = await roleManager.AddClaimAsync(
                role,
                new Claim(PermissionAuthorizationHandler.PermissionClaimType, permission));
            if (!add.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", add.Errors.Select(x => x.Description)));
            }
        }
    }
}
