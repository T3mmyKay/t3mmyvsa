using System.Text.RegularExpressions;

namespace T3mmyvsa.CLI.Scaffolding;

internal static class EntityParser
{
    private static readonly Regex PropertyRegex = new(
        @"public\s+(?<required>required\s+)?(?<type>[A-Za-z0-9_\.\?<>,\[\] ]+?)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\{\s*get;\s*(?<setter>set|init);",
        RegexOptions.CultureInvariant | RegexOptions.Multiline);

    private static readonly Regex CollectionTypeRegex = new(
        @"^(IEnumerable|ICollection|IReadOnlyCollection|IList|IReadOnlyList|List|HashSet|Dictionary|IDictionary|IReadOnlyDictionary)<",
        RegexOptions.CultureInvariant);

    public static EntityModel Parse(ProjectContext project, string entityPath)
    {
        var content = File.ReadAllText(entityPath);
        var entityNames = Directory
            .EnumerateFiles(Path.Combine(project.RootDirectory, "Entities"), "*.cs", SearchOption.TopDirectoryOnly)
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToHashSet(StringComparer.Ordinal);

        var properties = new List<EntityProperty>();
        var skippedNavigations = new List<string>();
        var unsupportedRequiredNavigations = new List<string>();

        foreach (Match match in PropertyRegex.Matches(content))
        {
            var type = NormalizeWhitespace(match.Groups["type"].Value);
            var name = match.Groups["name"].Value;
            var requiredKeyword = match.Groups["required"].Success;
            var setter = match.Groups["setter"].Value;

            if (ScaffoldConstants.InfrastructurePropertyNames.Contains(name))
            {
                continue;
            }

            if (IsCollectionNavigation(type))
            {
                skippedNavigations.Add(name);
                continue;
            }

            if (IsEntityReferenceNavigation(type, entityNames))
            {
                var nullableReference = type.Trim().EndsWith("?", StringComparison.Ordinal);
                if (requiredKeyword || !nullableReference)
                {
                    unsupportedRequiredNavigations.Add(name);
                }
                else
                {
                    skippedNavigations.Add(name);
                }

                continue;
            }

            properties.Add(new EntityProperty(type, name, setter == "set"));
        }

        return new EntityModel(
            properties,
            Regex.IsMatch(content, @":\s*AuditableEntity\b", RegexOptions.CultureInvariant),
            skippedNavigations,
            unsupportedRequiredNavigations);
    }

    private static bool IsCollectionNavigation(string type)
    {
        var withoutNullable = type.Trim().TrimEnd('?');

        return withoutNullable != "byte[]" &&
               (withoutNullable.EndsWith("[]", StringComparison.Ordinal) ||
                CollectionTypeRegex.IsMatch(withoutNullable));
    }

    private static bool IsEntityReferenceNavigation(string type, HashSet<string> entityNames)
    {
        var simpleType = type.Trim().TrimEnd('?');
        var genericIndex = simpleType.IndexOf('<');
        if (genericIndex >= 0)
        {
            simpleType = simpleType[..genericIndex];
        }

        var lastDot = simpleType.LastIndexOf('.');
        if (lastDot >= 0)
        {
            simpleType = simpleType[(lastDot + 1)..];
        }

        return entityNames.Contains(simpleType);
    }

    private static string NormalizeWhitespace(string value) =>
        Regex.Replace(value.Trim(), @"\s+", " ", RegexOptions.CultureInvariant);
}
