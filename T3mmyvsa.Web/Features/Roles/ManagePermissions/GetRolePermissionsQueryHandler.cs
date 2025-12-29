using T3mmyvsa.Authorization.Handlers;

namespace T3mmyvsa.Features.Roles.ManagePermissions;

public class GetRolePermissionsQueryHandler(RoleManager<IdentityRole> roleManager)
    : IQueryHandler<GetRolePermissionsQuery, List<string>>
{
    public async Task<List<string>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.RoleId) ?? throw new KeyNotFoundException("Role not found.");
        var claims = await roleManager.GetClaimsAsync(role);
        var permissions = claims
            .Where(c => c.Type == PermissionAuthorizationHandler.PermissionClaimType)
            .Select(c => c.Value)
            .ToList();

        return permissions;
    }
}
