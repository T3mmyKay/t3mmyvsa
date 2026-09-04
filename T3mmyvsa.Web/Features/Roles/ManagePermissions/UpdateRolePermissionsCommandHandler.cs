using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Authorization.Handlers;
using T3mmyvsa.Data;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Roles.ManagePermissions;

public class UpdateRolePermissionsCommandHandler(AppDbContext db)
    : ICommandHandler<UpdateRolePermissionsCommand>
{
    public async Task Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        if (request.Permissions is null)
        {
            throw new InvalidOperationException("Permissions are required.");
        }

        if (request.Permissions.Count != request.Permissions.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            throw new InvalidOperationException("Permission values must be unique.");
        }

        var knownPermissions = Enum.GetValues<AppPermission>()
            .Select(permission => permission.GetDescription())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unknown = request.Permissions.Where(permission => !knownPermissions.Contains(permission)).ToArray();
        if (unknown.Length > 0)
        {
            throw new InvalidOperationException($"Unknown permission(s): {string.Join(", ", unknown)}");
        }

        var role = await db.Roles.SingleOrDefaultAsync(x => x.Id == request.RoleId, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");

        const string claimType = PermissionAuthorizationHandler.PermissionClaimType;
        var existingClaims = await db.RoleClaims
            .Where(x => x.RoleId == role.Id && x.ClaimType == claimType)
            .ToListAsync(cancellationToken);

        var desired = request.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingValues = existingClaims
            .Where(x => x.ClaimValue is not null)
            .Select(x => x.ClaimValue!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        db.RoleClaims.RemoveRange(existingClaims.Where(x => x.ClaimValue is null || !desired.Contains(x.ClaimValue)));

        foreach (var permission in desired.Where(permission => !existingValues.Contains(permission)))
        {
            db.RoleClaims.Add(new IdentityRoleClaim<string>
            {
                RoleId = role.Id,
                ClaimType = claimType,
                ClaimValue = permission
            });
        }

        role.ConcurrencyStamp = Guid.NewGuid().ToString();

        // A single SaveChanges call is transactional, so the permission set is replaced atomically.
        await db.SaveChangesAsync(cancellationToken);
    }
}
