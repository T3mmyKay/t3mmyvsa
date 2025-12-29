using T3mmyvsa.Entities;
using T3mmyvsa.Models.Shared;

namespace T3mmyvsa.Features.Users.GetUsers;

public class GetUsersQueryHandler(UserManager<User> userManager) : IQueryHandler<GetUsersQuery, PaginatedResponse<UserResponse>>
{
    public async Task<PaginatedResponse<UserResponse>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var request = query.Request;

        var queryable = userManager.Users.AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            queryable = queryable.Where(u =>
                (u.Email != null && u.Email.Contains(search)) ||
                (u.UserName != null && u.UserName.Contains(search)) ||
                (u.FirstName != null && u.FirstName.Contains(search)) ||
                (u.LastName != null && u.LastName.Contains(search)) ||
                ((u.FirstName + " " + u.LastName).Contains(search)));
        }

        // Sorting
        queryable = request.SortOrder == SortOrder.Desc
            ? request.SortColumn switch
            {
                UserSortColumn.FirstName => queryable.OrderByDescending(u => u.FirstName),
                UserSortColumn.LastName => queryable.OrderByDescending(u => u.LastName),
                UserSortColumn.Email => queryable.OrderByDescending(u => u.Email),
                UserSortColumn.FullName => queryable.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName),
                _ => queryable.OrderByDescending(u => u.UserName)
            }
            : request.SortColumn switch
            {
                UserSortColumn.FirstName => queryable.OrderBy(u => u.FirstName),
                UserSortColumn.LastName => queryable.OrderBy(u => u.LastName),
                UserSortColumn.Email => queryable.OrderBy(u => u.Email),
                UserSortColumn.FullName => queryable.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
                _ => queryable.OrderBy(u => u.UserName)
            };

        // Project to Response DTO
        var responseQuery = queryable.Select(u => new UserResponse(
            u.Id,
            u.UserName,
            u.Email,
            u.FirstName,
            u.LastName,
            new List<string>(),
            new List<string>()
        ));

        // PagedList handles Skip/Take/Count
        var pagedList = await PagedList<UserResponse>.CreateAsync(responseQuery, request.Page ?? 1, request.PageSize ?? 15);

        var userResponses = pagedList.ToList();

        var meta = new PaginationMeta
        {
            CurrentPage = pagedList.CurrentPage,
            From = pagedList.TotalCount == 0 ? 0 : (pagedList.CurrentPage - 1) * pagedList.PageSize + 1,
            LastPage = pagedList.TotalPages,
            Path = "/api/v1/users",
            PerPage = pagedList.PageSize,
            To = pagedList.TotalCount == 0 ? 0 : (pagedList.CurrentPage - 1) * pagedList.PageSize + userResponses.Count,
            Total = pagedList.TotalCount
        };

        var links = new PaginationLinks
        {
            First = $"?page=1&per_page={pagedList.PageSize}&search={request.Search}&sort_column={request.SortColumn}&sort_order={request.SortOrder}",
            Last = $"?page={pagedList.TotalPages}&per_page={pagedList.PageSize}&search={request.Search}&sort_column={request.SortColumn}&sort_order={request.SortOrder}",
            Prev = pagedList.HasPrevious ? $"?page={pagedList.CurrentPage - 1}&per_page={pagedList.PageSize}&search={request.Search}&sort_column={request.SortColumn}&sort_order={request.SortOrder}" : null,
            Next = pagedList.HasNext ? $"?page={pagedList.CurrentPage + 1}&per_page={pagedList.PageSize}&search={request.Search}&sort_column={request.SortColumn}&sort_order={request.SortOrder}" : null
        };

        return new PaginatedResponse<UserResponse>(userResponses, meta, links);
    }
}
