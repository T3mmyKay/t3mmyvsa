using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Data;
using T3mmyvsa.Exceptions;

namespace T3mmyvsa.Features.Roles.DeleteRole;

public class DeleteRoleCommandHandler(RoleManager<IdentityRole> roleManager, AppDbContext db)
    : ICommandHandler<DeleteRoleCommand, DeleteRoleResponse>
{
    private static readonly HashSet<string> ProtectedRoles = Enum.GetNames<AppRole>()
        .Select(name => name.ToUpperInvariant())
        .ToHashSet(StringComparer.Ordinal);

    public async Task<DeleteRoleResponse> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.RoleId)
            ?? throw new KeyNotFoundException("Role not found.");

        if (role.NormalizedName is not null && ProtectedRoles.Contains(role.NormalizedName))
        {
            throw new ConflictException($"Cannot delete protected system role '{role.Name}'.");
        }

        if (await db.UserRoles.AsNoTracking().AnyAsync(x => x.RoleId == role.Id, cancellationToken))
        {
            throw new ConflictException($"Cannot delete role '{role.Name}' while it is assigned to users. Reassign those users first.");
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return new DeleteRoleResponse(true, $"Role '{role.Name}' has been deleted successfully.");
    }
}
