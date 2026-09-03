using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.ManagePermissions;

public class GetUserPermissionsQueryHandler(
    UserManager<User> userManager,
    ICurrentUserService currentUserService,
    IUserPermissionService userPermissionService)
    : IQueryHandler<GetUserPermissionsQuery, List<string>>
{
    public async Task<List<string>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId == "me" ? currentUserService.UserId : request.UserId;
        if (string.IsNullOrWhiteSpace(targetUserId))
        {
            throw new KeyNotFoundException("User not found.");
        }

        _ = await userManager.FindByIdAsync(targetUserId) ?? throw new KeyNotFoundException("User not found.");
        var permissions = await userPermissionService.GetPermissionsAsync(targetUserId, cancellationToken);
        return permissions.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
