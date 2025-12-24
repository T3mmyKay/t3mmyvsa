using Microsoft.EntityFrameworkCore.Metadata.Builders;
using T3mmyvsa.Entities;

namespace T3mmyvsa.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TableName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PrimaryKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasMaxLength(100);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.OldValues);

        builder.Property(x => x.NewValues);

        builder.Property(x => x.Timestamp)
            .IsRequired();

        // Indexes for common queries
        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.TableName, x.PrimaryKey });
    }
}
