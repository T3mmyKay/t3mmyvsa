using T3mmyvsa.Authorization.Enums;

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
            throw new InvalidOperationException($"Cannot rename protected system role '{role.Name}'.");
        }

        role.Name = request.RoleName.Trim();
        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return new UpdateRoleResponse(role.Id, role.Name!);
    }
}
