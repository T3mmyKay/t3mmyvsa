using T3mmyvsa.Entities.Base;

namespace T3mmyvsa.Entities;

/// <summary>
/// Audit log entity to track changes to entities.
/// Stores old and new values as JSON.
/// </summary>
public class AuditLog : BaseEntity
{
    public string? UserId { get; set; }
    public string Type { get; set; } = string.Empty; // Insert, Update, Delete
    public string TableName { get; set; } = string.Empty;
    public string PrimaryKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Snapshots of changed properties (JSON).
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// Snapshots of new values (JSON).
    /// </summary>
    public string? NewValues { get; set; }
    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
