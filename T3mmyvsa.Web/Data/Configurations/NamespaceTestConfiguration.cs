using Microsoft.EntityFrameworkCore.Metadata.Builders;
using T3mmyvsa.Entities;

namespace T3mmyvsa.Data.Configurations;

public class NamespaceTestConfiguration : IEntityTypeConfiguration<NamespaceTest>
{
    public void Configure(EntityTypeBuilder<NamespaceTest> builder)
    {
        builder.ToTable("NamespaceTests");
    }
}