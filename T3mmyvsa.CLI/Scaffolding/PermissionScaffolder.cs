using System.Text;
using System.Text.RegularExpressions;

namespace T3mmyvsa.CLI.Scaffolding;

internal static class PermissionScaffolder
{
    public static void AddCrudPermissions(ProjectContext project, string pluralName)
    {
        var path = Path.Combine(project.RootDirectory, "Authorization", "Enums", "AppPermission.cs");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("AppPermission.cs was not found.", path);
        }

        var content = File.ReadAllText(path);
        var permissions = new[]
        {
            (Name: $"{pluralName}View", Description: $"{pluralName}.View"),
            (Name: $"{pluralName}Create", Description: $"{pluralName}.Create"),
            (Name: $"{pluralName}Update", Description: $"{pluralName}.Update"),
            (Name: $"{pluralName}Delete", Description: $"{pluralName}.Delete")
        };

        var missing = permissions
            .Where(permission => !Regex.IsMatch(
                content,
                $@"\b{Regex.Escape(permission.Name)}\b",
                RegexOptions.CultureInvariant))
            .ToList();

        if (missing.Count == 0)
        {
            Console.WriteLine("Permissions already exist. No AppPermission changes required.");
            return;
        }

        var closingBraceIndex = content.LastIndexOf('}');
        if (closingBraceIndex < 0)
        {
            throw new InvalidOperationException("AppPermission.cs does not contain a closing enum brace.");
        }

        var beforeBrace = content[..closingBraceIndex].TrimEnd();
        if (!beforeBrace.EndsWith(",", StringComparison.Ordinal) &&
            !beforeBrace.EndsWith("{", StringComparison.Ordinal))
        {
            beforeBrace += ",";
        }

        var builder = new StringBuilder(beforeBrace);
        builder.AppendLine();
        builder.AppendLine();

        foreach (var permission in missing)
        {
            builder.AppendLine($"    [Description(\"{permission.Description}\")]");
            builder.AppendLine($"    {permission.Name},");
            builder.AppendLine();
        }

        builder.Append('}');
        builder.AppendLine();

        ScaffoldFileWriter.WriteTextAtomically(path, builder.ToString());
        Console.WriteLine($"Added {missing.Count} permission(s) to Authorization/Enums/AppPermission.cs.");
    }
}
