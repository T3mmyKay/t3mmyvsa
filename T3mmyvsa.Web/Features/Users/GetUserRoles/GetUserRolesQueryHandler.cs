using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.GetUserRoles;

public class GetUserRolesQueryHandler(
    UserManager<User> userManager,
    ICurrentUserService currentUserService
) : IQueryHandler<GetUserRolesQuery, List<string>>
{
    public async Task<List<string>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId == "me" ? currentUserService.UserId : request.UserId;

        if (string.IsNullOrEmpty(targetUserId))
        {
            throw new KeyNotFoundException("User not found.");
        }

        var user = await userManager.FindByIdAsync(targetUserId) ?? throw new KeyNotFoundException("User not found.");
        var roles = await userManager.GetRolesAsync(user);
        return [.. roles];
    }
}