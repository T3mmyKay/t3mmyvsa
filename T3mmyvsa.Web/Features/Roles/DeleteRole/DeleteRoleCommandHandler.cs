using T3mmyvsa.Authorization.Enums;

namespace T3mmyvsa.Features.Roles.DeleteRole;

public class DeleteRoleCommandHandler(RoleManager<IdentityRole> roleManager)
    : ICommandHandler<DeleteRoleCommand, DeleteRoleResponse>
{
    // Protected roles that cannot be deleted (from AppRole enum)
    private static readonly HashSet<string> ProtectedRoles = Enum.GetNames<AppRole>()
        .Select(name => name.ToUpperInvariant())
        .ToHashSet();

    public async Task<DeleteRoleResponse> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.RoleId) ?? throw new InvalidOperationException("Role not found.");

        // Check if this is a protected system role
        if (role.NormalizedName is not null && ProtectedRoles.Contains(role.NormalizedName))
        {
            throw new InvalidOperationException($"Cannot delete protected system role '{role.Name}'.");
        }

        // Remove all claims associated with this role to avoid orphan entities
        var roleClaims = await roleManager.GetClaimsAsync(role);
        foreach (var claim in roleClaims)
        {
            await roleManager.RemoveClaimAsync(role, claim);
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return new DeleteRoleResponse(true, $"Role '{role.Name}' has been deleted successfully.");
    }
}
