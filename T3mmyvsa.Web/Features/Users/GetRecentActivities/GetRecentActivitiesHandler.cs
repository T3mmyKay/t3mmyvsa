using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Data;
using T3mmyvsa.Entities;
using T3mmyvsa.Exceptions;
using T3mmyvsa.Extensions;
using T3mmyvsa.Interfaces;
using T3mmyvsa.Models.Shared;

namespace T3mmyvsa.Features.Users.GetRecentActivities;

public class GetRecentActivitiesHandler(
    AppDbContext context,
    ICurrentUserService currentUserService,
    IUserPermissionService userPermissionService,
    IHttpContextAccessor httpContextAccessor)
    : IQueryHandler<GetRecentActivitiesQuery, PaginatedResponse<RecentActivityResponse>>
{
    public async Task<PaginatedResponse<RecentActivityResponse>> Handle(
        GetRecentActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        var actorUserId = currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            throw new UnauthorizedAccessException("Authentication is required.");
        }

        var targetUserId = string.IsNullOrWhiteSpace(request.UserId) ? actorUserId : request.UserId.Trim();

        if (!string.Equals(targetUserId, actorUserId, StringComparison.Ordinal))
        {
            var permissions = await userPermissionService.GetPermissionsAsync(actorUserId, cancellationToken);
            if (!permissions.Contains(AppPermission.UsersViewActivity.GetDescription()))
            {
                throw new ForbiddenException("You do not have permission to view another user's activity.");
            }
        }

        var source = context.AuditLogs
            .AsNoTracking()
            .Where(x => x.UserId == targetUserId)
            .OrderByDescending(x => x.Timestamp)
            .ThenByDescending(x => x.Id);

        var paged = await PagedList<AuditLog>.CreateAsync(
            source,
            request.Page ?? 1,
            request.PageSize ?? 25,
            cancellationToken);

        var data = paged.Select(x => new RecentActivityResponse(
            x.Id,
            x.Type,
            x.TableName,
            x.PrimaryKey,
            x.OldValues,
            x.NewValues,
            x.IpAddress,
            x.UserAgent,
            x.Timestamp)).ToList();

        var path = httpContextAccessor.HttpContext?.Request.Path.Value ?? "/api/v1/users/activities";
        var userFilter = string.IsNullOrWhiteSpace(request.UserId)
            ? string.Empty
            : $"&userId={Uri.EscapeDataString(request.UserId.Trim())}";

        string BuildLink(int page) => $"{path}?page={page}&per_page={paged.PageSize}{userFilter}";

        var meta = new PaginationMeta
        {
            CurrentPage = paged.CurrentPage,
            From = paged.TotalCount == 0 ? null : (paged.CurrentPage - 1) * paged.PageSize + 1,
            To = paged.TotalCount == 0 ? null : (paged.CurrentPage - 1) * paged.PageSize + data.Count,
            LastPage = paged.TotalPages,
            Path = path,
            PerPage = paged.PageSize,
            Total = paged.TotalCount
        };

        var links = new PaginationLinks
        {
            First = BuildLink(1),
            Last = BuildLink(Math.Max(paged.TotalPages, 1)),
            Prev = paged.HasPrevious ? BuildLink(paged.CurrentPage - 1) : null,
            Next = paged.HasNext ? BuildLink(paged.CurrentPage + 1) : null
        };

        return new PaginatedResponse<RecentActivityResponse>(data, meta, links);
    }
}
