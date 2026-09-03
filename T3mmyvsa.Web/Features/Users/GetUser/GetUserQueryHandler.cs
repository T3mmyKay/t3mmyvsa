using T3mmyvsa.Entities;
using T3mmyvsa.Features.Users.GetUsers;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.GetUser;

public class GetUserQueryHandler(UserManager<User> userManager, IUserPermissionService userPermissionService)
    : IQueryHandler<GetUserQuery, UserResponse?>
{
    public async Task<UserResponse?> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(query.Id);
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        var permissions = await userPermissionService.GetPermissionsAsync(user.Id, cancellationToken);

        return new UserResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            [.. roles],
            [.. permissions.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)]);
    }
}
