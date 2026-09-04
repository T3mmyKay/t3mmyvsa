using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using T3mmyvsa.Models.Shared;

namespace T3mmyvsa.Features.Users.GetUsers;

public record GetUsersQuery(
    [FromQuery(Name = "search")] string? Search,
    [FromQuery(Name = "sort_column")] UserSortColumn? SortColumn,
    [FromQuery(Name = "sort_order")] SortOrder? SortOrder,
    [FromQuery(Name = "page"), Range(1, int.MaxValue)] int? Page,
    [FromQuery(Name = "per_page"), Range(1, 100)] int? PageSize
) : IQuery<PaginatedResponse<UserResponse>>;
