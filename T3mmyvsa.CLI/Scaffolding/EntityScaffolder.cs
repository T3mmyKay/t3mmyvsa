using System.Text.RegularExpressions;

namespace T3mmyvsa.CLI.Scaffolding;

internal static class EntityScaffolder
{
    public static void Scaffold(
        ProjectContext project,
        string entityName,
        bool overwrite,
        bool baseOnly)
    {
        var pluralName = Naming.Pluralize(entityName);
        var baseType = baseOnly ? "BaseEntity" : "AuditableEntity";

        Console.WriteLine($"Scaffolding {entityName} in {project.RootDirectory}");
        Console.WriteLine($"Root namespace: {project.RootNamespace}");
        Console.WriteLine($"Entity base: {baseType}");

        var entityPath = Path.Combine(project.RootDirectory, "Entities", $"{entityName}.cs");
        var configurationPath = Path.Combine(
            project.RootDirectory,
            "Data",
            "Configurations",
            $"{entityName}Configuration.cs");

        var entityContent = $$"""
using {{project.RootNamespace}}.Entities.Base;

namespace {{project.RootNamespace}}.Entities;

public sealed class {{entityName}} : {{baseType}}
{
}
""";

        var configurationContent = $$"""
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {{project.RootNamespace}}.Entities;

namespace {{project.RootNamespace}}.Data.Configurations;

public sealed class {{entityName}}Configuration : IEntityTypeConfiguration<{{entityName}}>
{
    public void Configure(EntityTypeBuilder<{{entityName}}> builder)
    {
        builder.ToTable("{{pluralName}}");
    }
}
""";

        ScaffoldFileWriter.WriteGeneratedFile(entityPath, entityContent, overwrite);
        ScaffoldFileWriter.WriteGeneratedFile(configurationPath, configurationContent, overwrite);
        EnsureDbSet(project, entityName, pluralName);

        Console.WriteLine("Entity scaffolding complete.");
    }

    private static void EnsureDbSet(ProjectContext project, string entityName, string pluralName)
    {
        var path = Path.Combine(project.RootDirectory, "Data", "AppDbContext.cs");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("AppDbContext.cs was not found.", path);
        }

        var content = File.ReadAllText(path);
        if (Regex.IsMatch(
            content,
            $@"DbSet\s*<\s*{Regex.Escape(entityName)}\s*>",
            RegexOptions.CultureInvariant))
        {
            Console.WriteLine($"DbSet<{entityName}> already exists. No AppDbContext change required.");
            return;
        }

        var insertAt = content.IndexOf("protected override void OnModelCreating", StringComparison.Ordinal);
        if (insertAt < 0)
        {
            throw new InvalidOperationException("Could not locate OnModelCreating in AppDbContext.cs.");
        }

        var declaration =
            $"    public DbSet<{entityName}> {pluralName} => Set<{entityName}>();" +
            Environment.NewLine +
            Environment.NewLine;

        var updated = content.Insert(insertAt, declaration);
        ScaffoldFileWriter.WriteTextAtomically(path, updated);
        Console.WriteLine("Updated Data/AppDbContext.cs.");
    }
}
