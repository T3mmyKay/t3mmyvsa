using T3mmyvsa.Attributes;
using T3mmyvsa.Authorization.Handlers;
using T3mmyvsa.Data;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Services;

[ScopedService]
public sealed class UserPermissionService(AppDbContext db) : IUserPermissionService
{
    public async Task<IReadOnlySet<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        const string permissionClaimType = PermissionAuthorizationHandler.PermissionClaimType;

        var rolePermissions = db.UserRoles.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Join(
                db.RoleClaims.AsNoTracking().Where(x => x.ClaimType == permissionClaimType),
                userRole => userRole.RoleId,
                roleClaim => roleClaim.RoleId,
                (_, roleClaim) => roleClaim.ClaimValue);

        var directPermissions = db.UserClaims.AsNoTracking()
            .Where(x => x.UserId == userId && x.ClaimType == permissionClaimType)
            .Select(x => x.ClaimValue);

        var values = await rolePermissions
            .Union(directPermissions)
            .Where(x => x != null)
            .Select(x => x!)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
    }
}
