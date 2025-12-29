using Microsoft.EntityFrameworkCore;
using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Authorization.Handlers;
using T3mmyvsa.Data;
using T3mmyvsa.Extensions;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.GetRecentActivities;

public class GetRecentActivitiesHandler(
    AppDbContext context,
    ICurrentUserService currentUserService,
    IHttpContextAccessor httpContextAccessor
) : IQueryHandler<GetRecentActivitiesQuery, List<RecentActivityResponse>>
{
    public async Task<List<RecentActivityResponse>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId;

        // If no UserId provided, use current user
        if (string.IsNullOrEmpty(targetUserId))
        {
            targetUserId = currentUserService.UserId;
        }

        if (string.IsNullOrEmpty(targetUserId))
        {
            return [];
        }

        // Authorization Check
        // If requesting another user's data, check for ViewActivity permission
        if (targetUserId != currentUserService.UserId)
        {
            var user = httpContextAccessor.HttpContext?.User;
            var requiredPermission = AppPermission.UsersViewActivity.GetDescription();

            if (user == null || !user.HasClaim(c => c.Type == PermissionAuthorizationHandler.PermissionClaimType && c.Value == requiredPermission))
            {
                return [];
            }
        }

        var activities = await context.AuditLogs
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
                x.Timestamp
            ))
            .ToListAsync(cancellationToken);

        return activities;
    }
}
