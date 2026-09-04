namespace T3mmyvsa.CLI.Scaffolding;

internal static class FeatureScaffolder
{
    public static void Scaffold(ProjectContext project, string entityName, bool overwrite)
    {
        var pluralName = Naming.Pluralize(entityName);
        var route = Naming.ToKebabCase(pluralName);
        var entityPath = Path.Combine(project.RootDirectory, "Entities", $"{entityName}.cs");

        if (!File.Exists(entityPath))
        {
            throw new FileNotFoundException(
                $"Entity '{entityName}' was not found. Run make:entity first or add the entity before scaffolding features.",
                entityPath);
        }

        var entityModel = EntityParser.Parse(project, entityPath);

        if (entityModel.UnsupportedRequiredNavigationProperties.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot safely scaffold '{entityName}'. Required navigation properties are not supported in generic CRUD input: " +
                string.Join(", ", entityModel.UnsupportedRequiredNavigationProperties) +
                ". Model these relationships with scalar foreign-key properties or implement the slice manually.");
        }

        foreach (var skipped in entityModel.SkippedNavigationProperties)
        {
            Console.WriteLine(
                $"Warning: navigation property '{skipped}' is not included in generated transport contracts.");
        }

        var featureRoot = Path.Combine(project.RootDirectory, "Features", pluralName);
        var files = FeatureTemplateFactory.Build(
            project.RootNamespace,
            entityName,
            pluralName,
            route,
            entityModel.Properties,
            entityModel.IsAuditable);

        var existing = files
            .Select(file => Path.Combine(featureRoot, file.RelativePath))
            .Where(File.Exists)
            .Select(path => Path.GetRelativePath(project.RootDirectory, path))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();

        if (existing.Count > 0 && !overwrite)
        {
            throw new InvalidOperationException(
                "Feature scaffolding would overwrite existing files. Re-run with --force only if replacement is intentional:" +
                Environment.NewLine +
                string.Join(Environment.NewLine, existing.Select(path => $"  - {path}")));
        }

        foreach (var file in files)
        {
            ScaffoldFileWriter.WriteGeneratedFile(
                Path.Combine(featureRoot, file.RelativePath),
                file.Content,
                overwrite);
        }

        PermissionScaffolder.AddCrudPermissions(project, pluralName);

        Console.WriteLine($"Feature scaffolding complete for {entityName}.");
        Console.WriteLine($"Generated {files.Count} files under Features/{pluralName}.");
    }
}
