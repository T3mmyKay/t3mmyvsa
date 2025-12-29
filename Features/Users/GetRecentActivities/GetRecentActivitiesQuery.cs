

namespace T3mmyvsa.Features.Users.GetRecentActivities;

public record GetRecentActivitiesQuery(string? UserId) : IQuery<List<RecentActivityResponse>>;
