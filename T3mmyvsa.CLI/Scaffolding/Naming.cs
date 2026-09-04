using System.Text.RegularExpressions;

namespace T3mmyvsa.CLI.Scaffolding;

internal static class Naming
{
    public static void ValidateEntityName(string entityName)
    {
        if (!Regex.IsMatch(entityName, @"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant))
        {
            throw new ArgumentException(
                "Entity name must be a valid C# identifier containing only letters, digits, and underscores.");
        }

        if (!char.IsUpper(entityName[0]))
        {
            throw new ArgumentException("Entity name must use PascalCase and begin with an uppercase letter.");
        }

        if (ScaffoldConstants.CSharpKeywords.Contains(entityName))
        {
            throw new ArgumentException($"'{entityName}' is a reserved C# keyword and cannot be used as an entity name.");
        }
    }

    public static string Pluralize(string singular)
    {
        if (Regex.IsMatch(singular, @"[^aeiou]y$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            return singular[..^1] + "ies";
        }

        if (Regex.IsMatch(singular, @"(s|x|z|ch|sh)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            return singular + "es";
        }

        return singular + "s";
    }

    public static string ToKebabCase(string value)
    {
        var acronymBoundaries = Regex.Replace(
            value,
            "([A-Z]+)([A-Z][a-z])",
            "$1-$2",
            RegexOptions.CultureInvariant);

        var wordBoundaries = Regex.Replace(
            acronymBoundaries,
            "([a-z0-9])([A-Z])",
            "$1-$2",
            RegexOptions.CultureInvariant);

        return wordBoundaries.Replace('_', '-').ToLowerInvariant();
    }
}
