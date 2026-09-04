using T3mmyvsa.Models.Shared;

namespace T3mmyvsa.Features.Users.GetRecentActivities;

public sealed record GetRecentActivitiesQuery : IQuery<PaginatedResponse<RecentActivityResponse>>
{
    [FromQuery(Name = "userId")]
    public string? UserId { get; init; }

    [FromQuery(Name = "page")]
    public int? Page { get; init; } = 1;

    [FromQuery(Name = "per_page")]
    public int? PageSize { get; init; } = 25;
}
