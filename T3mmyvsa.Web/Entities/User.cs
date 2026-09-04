namespace T3mmyvsa.Entities;

using T3mmyvsa.Entities.Base;
using Microsoft.AspNetCore.Identity;

public class User : IdentityUser, IAuditableEntity
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
