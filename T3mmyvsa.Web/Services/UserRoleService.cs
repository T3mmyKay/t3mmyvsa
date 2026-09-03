using T3mmyvsa.Attributes;
using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Services;

[ScopedService]
public sealed class UserRoleService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager) : IUserRoleService
{
    public async Task SetExactRoleAsync(User user, string roleName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

        if (!Enum.TryParse<AppRole>(roleName, ignoreCase: true, out var appRole))
        {
            throw new InvalidOperationException(
                $"Role '{roleName}' is not a valid application role. Allowed: {string.Join(", ", Enum.GetNames<AppRole>())}.");
        }

        var canonicalRoleName = appRole.ToString();
        if (!await roleManager.RoleExistsAsync(canonicalRoleName))
        {
            throw new InvalidOperationException($"Role '{canonicalRoleName}' does not exist.");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count == 1 && string.Equals(currentRoles[0], canonicalRoleName, StringComparison.Ordinal))
        {
            return;
        }

        if (currentRoles.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", removeResult.Errors.Select(x => x.Description)));
            }
        }

        var addResult = await userManager.AddToRoleAsync(user, canonicalRoleName);
        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", addResult.Errors.Select(x => x.Description)));
        }
    }
}
