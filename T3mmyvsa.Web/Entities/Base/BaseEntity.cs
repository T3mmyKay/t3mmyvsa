namespace T3mmyvsa.Entities.Base;

/// <summary>
/// Base entity with time-ordered GUID primary key.
/// Uses Guid.CreateVersion7 for better database indexing performance.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
}
