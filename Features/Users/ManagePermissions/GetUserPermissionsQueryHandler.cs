using T3mmyvsa.Authorization.Handlers;
using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.ManagePermissions;

public class GetUserPermissionsQueryHandler(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    ICurrentUserService currentUserService
) : IQueryHandler<GetUserPermissionsQuery, List<string>>
{
    public async Task<List<string>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId == "me" ? currentUserService.UserId : request.UserId;

        if (string.IsNullOrEmpty(targetUserId))
        {
            throw new KeyNotFoundException("User not found.");
        }

        var user = await userManager.FindByIdAsync(targetUserId) ?? throw new KeyNotFoundException("User not found.");

        // 1. Get User Roles
        var roles = await userManager.GetRolesAsync(user);

        // 2. Aggregate permissions from all roles
        var allPermissions = new HashSet<string>();

        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var claims = await roleManager.GetClaimsAsync(role);
                foreach (var claim in claims.Where(c => c.Type == PermissionAuthorizationHandler.PermissionClaimType))
                {
                    allPermissions.Add(claim.Value);
                }
            }
        }

        // 3. (Optional) Add direct user claims if any
        var userClaims = await userManager.GetClaimsAsync(user);
        foreach (var claim in userClaims.Where(c => c.Type == PermissionAuthorizationHandler.PermissionClaimType))
        {
            allPermissions.Add(claim.Value);
        }

        return [.. allPermissions];
    }
}
