using T3mmyvsa.Models.Shared;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace T3mmyvsa.Features.Users.GetUsers;

public record GetUsersQuery(
    string? Search,
    UserSortColumn? SortColumn,
    SortOrder? SortOrder,
    int? Page,
    int? PageSize
) : IQuery<PaginatedResponse<UserResponse>>;
