namespace T3mmyvsa.Features.Users.GetRecentActivities;

public record RecentActivityResponse(
    string Type,
    string TableName,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset Timestamp
);
