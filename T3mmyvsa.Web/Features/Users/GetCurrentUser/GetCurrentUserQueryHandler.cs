using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.GetCurrentUser;

public class GetCurrentUserQueryHandler(
    UserManager<User> userManager,
    ICurrentUserService currentUserService,
    IUserPermissionService userPermissionService)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery query, CancellationToken ct)
    {
        var userId = currentUserService.UserId ?? throw new InvalidOperationException("User is not authenticated.");
        var user = await userManager.FindByIdAsync(userId) ?? throw new KeyNotFoundException("User not found.");
        var roles = await userManager.GetRolesAsync(user);
        var permissions = await userPermissionService.GetPermissionsAsync(user.Id, ct);

        return new CurrentUserResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            [.. roles],
            [.. permissions.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)],
            user.CreatedAt);
    }
}
