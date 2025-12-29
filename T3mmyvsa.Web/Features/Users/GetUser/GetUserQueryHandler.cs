using T3mmyvsa.Entities;
using T3mmyvsa.Features.Users.GetUsers;

namespace T3mmyvsa.Features.Users.GetUser;

public class GetUserQueryHandler(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager
) : IQueryHandler<GetUserQuery, UserResponse?>
{
    public async Task<UserResponse?> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(query.Id);

        if (user == null)
        {
            return null;
        }

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

        return new UserResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            [.. roles],
            [.. permissions]
        );
    }
}
