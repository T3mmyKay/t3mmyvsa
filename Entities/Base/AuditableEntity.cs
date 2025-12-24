namespace T3mmyvsa.Entities.Base;

/// <summary>
/// Auditable entity that tracks creation and modification metadata.
/// Inherits time-ordered GUID from BaseEntity.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
