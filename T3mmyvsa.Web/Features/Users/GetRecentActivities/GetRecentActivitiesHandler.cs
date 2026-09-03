using Microsoft.EntityFrameworkCore;
using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Data;
using T3mmyvsa.Extensions;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.GetRecentActivities;

public class GetRecentActivitiesHandler(
    AppDbContext context,
    ICurrentUserService currentUserService,
    IUserPermissionService userPermissionService
) : IQueryHandler<GetRecentActivitiesQuery, List<RecentActivityResponse>>
{
    public async Task<List<RecentActivityResponse>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
    {
        var actorUserId = currentUserService.UserId;
        var targetUserId = string.IsNullOrWhiteSpace(request.UserId) ? actorUserId : request.UserId;

        if (string.IsNullOrWhiteSpace(targetUserId))
        {
            return [];
        }

        // Self-access is allowed. Reading another user's activity uses the same server-side
        // effective-permission authority as endpoint authorization; JWT permission claims are not trusted.
        if (!string.Equals(targetUserId, actorUserId, StringComparison.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                return [];
            }

            var permissions = await userPermissionService.GetPermissionsAsync(actorUserId, cancellationToken);
            if (!permissions.Contains(AppPermission.UsersViewActivity.GetDescription()))
            {
                return [];
            }
        }

        return await context.AuditLogs
            .AsNoTracking()
            .Where(x => x.UserId == targetUserId)
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new RecentActivityResponse(
                x.Type,
                x.TableName,
                x.OldValues,
                x.NewValues,
                x.IpAddress,
                x.UserAgent,
                x.Timestamp))
            .ToListAsync(cancellationToken);
    }
}
