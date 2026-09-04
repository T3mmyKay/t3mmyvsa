using System.Text.RegularExpressions;

namespace T3mmyvsa.CLI.Scaffolding;

internal static class ProjectLocator
{
    public static ProjectContext Find(string startDirectory)
    {
        var root = FindRoot(startDirectory);
        return new ProjectContext(root, GetRootNamespace(root));
    }

    private static string FindRoot(string startDirectory)
    {
        var absoluteStart = Path.GetFullPath(startDirectory);
        var current = new DirectoryInfo(absoluteStart);

        while (current is not null)
        {
            if (IsProjectRoot(current.FullName))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        var directChildren = Directory
            .EnumerateDirectories(absoluteStart)
            .Where(IsProjectRoot)
            .ToList();

        return directChildren.Count switch
        {
            1 => directChildren[0],
            > 1 => throw new InvalidOperationException(
                "Multiple T3mmyVSA project roots were found. Run the command from the intended web project directory."),
            _ => throw new InvalidOperationException(
                "Could not find a T3mmyVSA project root. Expected a .csproj plus Entities, Data, Features, and Authorization folders.")
        };
    }

    private static bool IsProjectRoot(string directory)
    {
        return Directory.EnumerateFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly).Any() &&
               Directory.Exists(Path.Combine(directory, "Entities")) &&
               Directory.Exists(Path.Combine(directory, "Data")) &&
               Directory.Exists(Path.Combine(directory, "Features")) &&
               Directory.Exists(Path.Combine(directory, "Authorization"));
    }

    private static string GetRootNamespace(string projectRoot)
    {
        var projects = Directory
            .EnumerateFiles(projectRoot, "*.csproj", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();

        if (projects.Count == 0)
        {
            throw new InvalidOperationException("No .csproj file was found in the project root.");
        }

        if (projects.Count > 1)
        {
            throw new InvalidOperationException(
                "Multiple .csproj files were found in the project root. Run the CLI from a single web project.");
        }

        var content = File.ReadAllText(projects[0]);
        var match = Regex.Match(
            content,
            @"<RootNamespace>\s*(?<value>[^<]+?)\s*</RootNamespace>",
            RegexOptions.CultureInvariant);

        return match.Success
            ? match.Groups["value"].Value.Trim()
            : Path.GetFileNameWithoutExtension(projects[0]);
    }
}
