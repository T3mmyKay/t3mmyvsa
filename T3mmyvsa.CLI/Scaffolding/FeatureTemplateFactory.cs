namespace T3mmyvsa.CLI.Scaffolding;

internal static partial class FeatureTemplateFactory
{
    public static List<GeneratedFile> Build(
        string rootNamespace,
        string entityName,
        string pluralName,
        string route,
        IReadOnlyList<EntityProperty> properties,
        bool isAuditable)
    {
        var files = new List<GeneratedFile>();

        AddCreateTemplates(files, rootNamespace, entityName, pluralName, route, properties);
        AddUpdateTemplates(files, rootNamespace, entityName, pluralName, route, properties);
        AddDeleteTemplates(files, rootNamespace, entityName, pluralName, route);
        AddGetTemplates(files, rootNamespace, entityName, pluralName, route, properties, isAuditable);
        AddListTemplates(files, rootNamespace, entityName, pluralName, route, properties, isAuditable);
        AddBulkDeleteTemplates(files, rootNamespace, entityName, pluralName, route);

        return files;
    }

    private static string NamespaceFor(
        string rootNamespace,
        string pluralName,
        string featureName) =>
        $"{rootNamespace}.Features.{pluralName}.{featureName}";

    private static string Permission(string pluralName, string operation) =>
        $"{pluralName}{operation}";

    private static string RenderProperties(IEnumerable<EntityProperty> properties)
    {
        return string.Join(
            Environment.NewLine,
            properties.Select(property =>
                $"public {property.Type} {property.Name} {{ get; init; }}{GetPropertyInitializer(property.Type)}"));
    }

    private static string RenderObjectInitializer(IEnumerable<EntityProperty> properties, string source)
    {
        var values = properties
            .Select(property => $"{property.Name} = {source}.{property.Name},")
            .ToList();

        return values.Count == 0 ? string.Empty : string.Join(Environment.NewLine, values);
    }

    private static string RenderAssignments(
        IEnumerable<EntityProperty> properties,
        string target,
        string source)
    {
        var values = properties
            .Select(property => $"{target}.{property.Name} = {source}.{property.Name};")
            .ToList();

        return values.Count == 0
            ? "// This entity has no settable properties to update."
            : string.Join(Environment.NewLine, values);
    }

    private static string RenderResponseProperties(
        IEnumerable<EntityProperty> properties,
        bool isAuditable)
    {
        var values = properties
            .Select(property =>
                $"public {property.Type} {property.Name} {{ get; init; }}{GetPropertyInitializer(property.Type)}")
            .ToList();

        if (isAuditable)
        {
            values.Add("public DateTimeOffset CreatedAt { get; init; }");
            values.Add("public DateTimeOffset? UpdatedAt { get; init; }");
        }

        return string.Join(Environment.NewLine, values);
    }

    private static string RenderResponseAssignments(
        IEnumerable<EntityProperty> properties,
        bool isAuditable,
        string source)
    {
        var values = properties
            .Select(property => $"{property.Name} = {source}.{property.Name},")
            .ToList();

        if (isAuditable)
        {
            values.Add($"CreatedAt = {source}.CreatedAt,");
            values.Add($"UpdatedAt = {source}.UpdatedAt,");
        }

        return string.Join(Environment.NewLine, values);
    }

    private static string RenderValidationRules(IEnumerable<EntityProperty> properties)
    {
        var rules = new List<string>();

        foreach (var property in properties)
        {
            var normalized = property.Type.Replace(" ", string.Empty, StringComparison.Ordinal);

            if (normalized == "string")
            {
                rules.Add($"RuleFor(x => x.{property.Name}).NotEmpty();");
            }
            else if (normalized == "Guid")
            {
                rules.Add($"RuleFor(x => x.{property.Name}).NotEmpty();");
            }
        }

        return rules.Count == 0
            ? "// Add business-specific validation rules here."
            : string.Join(Environment.NewLine, rules);
    }

    private static string RenderSearch(IEnumerable<EntityProperty> properties)
    {
        var stringProperties = properties
            .Where(property =>
                property.Type.Replace(" ", string.Empty, StringComparison.Ordinal) is "string" or "string?")
            .ToList();

        if (stringProperties.Count == 0)
        {
            return """
if (!string.IsNullOrWhiteSpace(request.Search))
{
    throw new ArgumentException("Search is not available because this entity has no string properties.");
}
""";
        }

        var conditions = string.Join(
            Environment.NewLine + "                || ",
            stringProperties.Select(property =>
            {
                var nullableGuard = property.Type.Contains('?')
                    ? $"entity.{property.Name} != null && "
                    : string.Empty;

                return $"({nullableGuard}entity.{property.Name}.Contains(search))";
            }));

        return $$"""
if (!string.IsNullOrWhiteSpace(request.Search))
{
    var search = request.Search.Trim();
    queryable = queryable.Where(entity =>
                {{conditions}});
}
""";
    }

    private static string RenderSortCases(
        IReadOnlyCollection<EntityProperty> sortableProperties,
        bool isAuditable,
        bool descending,
        string sortColumnName)
    {
        var direction = descending ? "OrderByDescending" : "OrderBy";
        var cases = new List<string>();

        foreach (var property in sortableProperties)
        {
            cases.Add(
                $"{sortColumnName}.{property.Name} => queryable.{direction}(entity => entity.{property.Name}).ThenBy(entity => entity.Id),");
        }

        if (isAuditable)
        {
            cases.Add(
                $"{sortColumnName}.CreatedAt => queryable.{direction}(entity => entity.CreatedAt).ThenBy(entity => entity.Id),");
        }

        cases.Add($"_ => queryable.{direction}(entity => entity.Id),");
        return string.Join(Environment.NewLine, cases);
    }

    private static bool IsSortable(EntityProperty property)
    {
        var type = property.Type
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .TrimEnd('?');

        return ScaffoldConstants.SortableTypes.Contains(type);
    }

    private static string GetPropertyInitializer(string type)
    {
        var normalized = type.Replace(" ", string.Empty, StringComparison.Ordinal);

        if (normalized == "string")
        {
            return " = string.Empty;";
        }

        if (normalized.EndsWith("?", StringComparison.Ordinal) || IsKnownValueType(normalized))
        {
            return string.Empty;
        }

        return " = default!;";
    }

    private static bool IsKnownValueType(string type)
    {
        var normalized = type.TrimEnd('?');

        return ScaffoldConstants.SortableTypes.Contains(normalized) ||
               normalized is "DateOnly" or "TimeOnly" or "TimeSpan" or "char";
    }

    private static string Indent(string value, int spaces)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var prefix = new string(' ', spaces);
        return string.Join(
            Environment.NewLine,
            value.Split(Environment.NewLine).Select(line => prefix + line));
    }
}
