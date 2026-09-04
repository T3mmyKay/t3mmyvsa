using Microsoft.AspNetCore.Mvc;
using T3mmyvsa.Models.Shared;

namespace T3mmyvsa.Features.Users.GetUsers;

public sealed record GetUsersQuery : IQuery<PaginatedResponse<UserResponse>>
{
    [FromQuery(Name = "search")]
    public string? Search { get; init; }

    [FromQuery(Name = "sort_column")]
    public UserSortColumn? SortColumn { get; init; }

    [FromQuery(Name = "sort_order")]
    public SortOrder? SortOrder { get; init; }

    [FromQuery(Name = "page")]
    public int? Page { get; init; }

    [FromQuery(Name = "per_page")]
    public int? PageSize { get; init; }
}
