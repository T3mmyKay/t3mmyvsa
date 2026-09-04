namespace T3mmyvsa.Features.Users.GetRecentActivities;

public record RecentActivityResponse(
    Guid Id,
    string Type,
    string TableName,
    string PrimaryKey,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset Timestamp
);
