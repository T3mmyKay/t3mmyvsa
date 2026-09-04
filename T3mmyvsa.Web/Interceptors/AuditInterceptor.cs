using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using T3mmyvsa.Attributes;
using T3mmyvsa.Entities;
using T3mmyvsa.Entities.Base;

namespace T3mmyvsa.Interceptors;

[SingletonService]
public sealed class AuditInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    private static readonly HashSet<string> AuditMetadataProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(IAuditableEntity.CreatedAt),
        nameof(IAuditableEntity.CreatedBy),
        nameof(IAuditableEntity.UpdatedAt),
        nameof(IAuditableEntity.UpdatedBy)
    };

    private static readonly HashSet<string> AuditableUserProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(User.UserName),
        nameof(User.Email),
        nameof(User.PhoneNumber),
        nameof(User.EmailConfirmed),
        nameof(User.PhoneNumberConfirmed),
        nameof(User.TwoFactorEnabled),
        nameof(User.LockoutEnabled),
        nameof(User.LockoutEnd),
        nameof(User.FirstName),
        nameof(User.LastName),
        nameof(User.IsActive)
    };

    private static readonly string[] SensitivePropertyFragments =
    [
        "Password",
        "Secret",
        "Token",
        "Credential",
        "ApiKey",
        "PrivateKey",
        "SecurityStamp",
        "ConcurrencyStamp"
    ];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFieldsAndLog(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditFieldsAndLog(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void UpdateAuditFieldsAndLog(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var httpContext = httpContextAccessor.HttpContext;
        var actorUserId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var auditActor = actorUserId ?? "System";
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString();
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = auditActor;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = auditActor;
            }
        }

        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            if (entry.Entity is not BaseEntity
                && entry.Entity is not User
                && entry.Entity is not IdentityRole
                && entry.Entity is not IdentityRoleClaim<string>
                && entry.Entity is not IdentityUserRole<string>
                && entry.Entity is not IdentityUserClaim<string>)
            {
                continue;
            }

            var primaryKey = entry.Entity switch
            {
                BaseEntity baseEntity => baseEntity.Id.ToString(),
                User user => user.Id,
                IdentityRole role => role.Id,
                IdentityRoleClaim<string> roleClaim => $"{roleClaim.RoleId}:{roleClaim.ClaimType}:{roleClaim.ClaimValue}",
                IdentityUserRole<string> userRole => $"{userRole.UserId}:{userRole.RoleId}",
                IdentityUserClaim<string> userClaim => $"{userClaim.UserId}:{userClaim.ClaimType}:{userClaim.ClaimValue}",
                _ => null
            };

            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                continue;
            }

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary || property.Metadata.IsPrimaryKey())
                {
                    continue;
                }

                var propertyName = property.Metadata.Name;
                if (!ShouldAuditProperty(entry.Entity, propertyName, property.Metadata.PropertyInfo))
                {
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;
                    case EntityState.Modified when property.IsModified:
                        oldValues[propertyName] = property.OriginalValue;
                        newValues[propertyName] = property.CurrentValue;
                        break;
                }
            }

            if (entry.State == EntityState.Modified && oldValues.Count == 0 && newValues.Count == 0)
            {
                continue;
            }

            auditEntries.Add(new AuditLog
            {
                UserId = actorUserId,
                Type = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Modified => "Updated",
                    EntityState.Deleted => "Deleted",
                    _ => entry.State.ToString()
                },
                TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                PrimaryKey = primaryKey,
                Timestamp = now,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues),
                NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues)
            });
        }

        if (auditEntries.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }
    }

    private static bool ShouldAuditProperty(object entity, string propertyName, System.Reflection.PropertyInfo? propertyInfo)
    {
        if (AuditMetadataProperties.Contains(propertyName))
        {
            return false;
        }

        if (propertyInfo?.IsDefined(typeof(AuditIgnoreAttribute), inherit: true) == true)
        {
            return false;
        }

        if (SensitivePropertyFragments.Any(fragment => propertyName.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return entity switch
        {
            User => AuditableUserProperties.Contains(propertyName),
            IdentityRole => propertyName is nameof(IdentityRole.Name) or nameof(IdentityRole.NormalizedName),
            IdentityRoleClaim<string> => propertyName is nameof(IdentityRoleClaim<string>.RoleId)
                or nameof(IdentityRoleClaim<string>.ClaimType)
                or nameof(IdentityRoleClaim<string>.ClaimValue),
            IdentityUserRole<string> => propertyName is nameof(IdentityUserRole<string>.UserId)
                or nameof(IdentityUserRole<string>.RoleId),
            IdentityUserClaim<string> => propertyName is nameof(IdentityUserClaim<string>.UserId)
                or nameof(IdentityUserClaim<string>.ClaimType)
                or nameof(IdentityUserClaim<string>.ClaimValue),
            BaseEntity => true,
            _ => false
        };
    }
}
