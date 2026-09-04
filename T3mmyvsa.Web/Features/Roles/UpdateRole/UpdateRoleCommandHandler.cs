using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Exceptions;

namespace T3mmyvsa.Features.Roles.UpdateRole;

public class UpdateRoleCommandHandler(RoleManager<IdentityRole> roleManager)
    : ICommandHandler<UpdateRoleCommand, UpdateRoleResponse>
{
    private static readonly HashSet<string> ProtectedRoles = Enum.GetNames<AppRole>()
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public async Task<UpdateRoleResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Role not found.");

        if (role.Name is not null && ProtectedRoles.Contains(role.Name))
        {
            throw new ConflictException($"Cannot rename protected system role '{role.Name}'.");
        }

        var roleName = request.RoleName.Trim();
        var existing = await roleManager.FindByNameAsync(roleName);
        if (existing is not null && !string.Equals(existing.Id, role.Id, StringComparison.Ordinal))
        {
            throw new ConflictException($"Role '{roleName}' already exists.");
        }

        role.Name = roleName;
        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return new UpdateRoleResponse(role.Id, role.Name!);
    }
}
