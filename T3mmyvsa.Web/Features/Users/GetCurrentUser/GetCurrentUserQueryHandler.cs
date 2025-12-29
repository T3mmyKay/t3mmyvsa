using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.GetCurrentUser;

public class GetCurrentUserQueryHandler(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    ICurrentUserService currentUserService
) : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery query, CancellationToken ct)
    {
        var userId = currentUserService.UserId ?? throw new InvalidOperationException("User is not authenticated.");
        var user = await userManager.FindByIdAsync(userId) ?? throw new KeyNotFoundException("User not found.");

        var roles = await userManager.GetRolesAsync(user);

        var permissions = new HashSet<string>();
        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var claims = await roleManager.GetClaimsAsync(role);
                foreach (var claim in claims.Where(c => c.Type == "Permission"))
                {
                    permissions.Add(claim.Value);
                }
            }
        }

        return new CurrentUserResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            [.. roles],
            [.. permissions],
            user.CreatedAt
        );
    }
}
