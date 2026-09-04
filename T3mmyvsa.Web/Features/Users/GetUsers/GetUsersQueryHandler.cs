using T3mmyvsa.Authorization.Handlers;
using T3mmyvsa.Data;
using T3mmyvsa.Entities;
using T3mmyvsa.Models.Shared;

namespace T3mmyvsa.Features.Users.GetUsers;

public class GetUsersQueryHandler(
    UserManager<User> userManager,
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor)
    : IQueryHandler<GetUsersQuery, PaginatedResponse<UserResponse>>
{
    public async Task<PaginatedResponse<UserResponse>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var queryable = userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            queryable = queryable.Where(u =>
                (u.Email != null && u.Email.Contains(search)) ||
                (u.UserName != null && u.UserName.Contains(search)) ||
                (u.FirstName != null && u.FirstName.Contains(search)) ||
                (u.LastName != null && u.LastName.Contains(search)) ||
                ((u.FirstName + " " + u.LastName).Contains(search)));
        }

        queryable = query.SortOrder == SortOrder.Desc
            ? query.SortColumn switch
            {
                UserSortColumn.FirstName => queryable.OrderByDescending(u => u.FirstName).ThenBy(u => u.Id),
                UserSortColumn.LastName => queryable.OrderByDescending(u => u.LastName).ThenBy(u => u.Id),
                UserSortColumn.Email => queryable.OrderByDescending(u => u.Email).ThenBy(u => u.Id),
                UserSortColumn.FullName => queryable.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName).ThenBy(u => u.Id),
                _ => queryable.OrderByDescending(u => u.UserName).ThenBy(u => u.Id)
            }
            : query.SortColumn switch
            {
                UserSortColumn.FirstName => queryable.OrderBy(u => u.FirstName).ThenBy(u => u.Id),
                UserSortColumn.LastName => queryable.OrderBy(u => u.LastName).ThenBy(u => u.Id),
                UserSortColumn.Email => queryable.OrderBy(u => u.Email).ThenBy(u => u.Id),
                UserSortColumn.FullName => queryable.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ThenBy(u => u.Id),
                _ => queryable.OrderBy(u => u.UserName).ThenBy(u => u.Id)
            };

        var page = query.Page ?? 1;
        var pageSize = query.PageSize ?? 15;
        var pagedUsers = await PagedList<User>.CreateAsync(queryable, page, pageSize, cancellationToken);
        var userIds = pagedUsers.Select(x => x.Id).ToList();

        var roleRows = userIds.Count == 0
            ? []
            : await db.UserRoles
                .AsNoTracking()
                .Where(x => userIds.Contains(x.UserId))
                .Join(
                    db.Roles.AsNoTracking(),
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (userRole, role) => new { userRole.UserId, role.Name })
                .Where(x => x.Name != null)
                .ToListAsync(cancellationToken);

        var roleMap = roleRows
            .GroupBy(x => x.UserId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => x.Name!).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(),
                StringComparer.Ordinal);

        const string permissionClaimType = PermissionAuthorizationHandler.PermissionClaimType;
        var rolePermissionRows = userIds.Count == 0
            ? []
            : await db.UserRoles
                .AsNoTracking()
                .Where(x => userIds.Contains(x.UserId))
                .Join(
                    db.RoleClaims.AsNoTracking().Where(x => x.ClaimType == permissionClaimType),
                    userRole => userRole.RoleId,
                    roleClaim => roleClaim.RoleId,
                    (userRole, roleClaim) => new { userRole.UserId, roleClaim.ClaimValue })
                .Where(x => x.ClaimValue != null)
                .ToListAsync(cancellationToken);

        var directPermissionRows = userIds.Count == 0
            ? []
            : await db.UserClaims
                .AsNoTracking()
                .Where(x => userIds.Contains(x.UserId) && x.ClaimType == permissionClaimType && x.ClaimValue != null)
                .Select(x => new { x.UserId, x.ClaimValue })
                .ToListAsync(cancellationToken);

        var permissionMap = rolePermissionRows
            .Concat(directPermissionRows)
            .GroupBy(x => x.UserId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => x.ClaimValue!).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(),
                StringComparer.Ordinal);

        var userResponses = pagedUsers.Select(user =>
        {
            var roles = roleMap.GetValueOrDefault(user.Id) ?? [];
            var permissions = permissionMap.GetValueOrDefault(user.Id) ?? [];

            return new UserResponse(
                user.Id,
                user.UserName,
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                roles.FirstOrDefault(),
                user.IsActive,
                user.EmailConfirmed,
                user.CreatedAt,
                roles,
                permissions);
        }).ToList();

        var path = httpContextAccessor.HttpContext?.Request.Path.Value ?? "/api/v1/users";
        var meta = new PaginationMeta
        {
            CurrentPage = pagedUsers.CurrentPage,
            From = pagedUsers.TotalCount == 0 ? null : (pagedUsers.CurrentPage - 1) * pagedUsers.PageSize + 1,
            LastPage = pagedUsers.TotalPages,
            Path = path,
            PerPage = pagedUsers.PageSize,
            To = pagedUsers.TotalCount == 0 ? null : (pagedUsers.CurrentPage - 1) * pagedUsers.PageSize + userResponses.Count,
            Total = pagedUsers.TotalCount
        };

        string BuildLink(int targetPage)
        {
            var search = Uri.EscapeDataString(query.Search?.Trim() ?? string.Empty);
            return $"{path}?page={targetPage}&per_page={pagedUsers.PageSize}&search={search}&sort_column={query.SortColumn}&sort_order={query.SortOrder}";
        }

        var links = new PaginationLinks
        {
            First = BuildLink(1),
            Last = BuildLink(Math.Max(pagedUsers.TotalPages, 1)),
            Prev = pagedUsers.HasPrevious ? BuildLink(pagedUsers.CurrentPage - 1) : null,
            Next = pagedUsers.HasNext ? BuildLink(pagedUsers.CurrentPage + 1) : null
        };

        return new PaginatedResponse<UserResponse>(userResponses, meta, links);
    }
}
