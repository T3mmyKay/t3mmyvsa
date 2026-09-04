namespace T3mmyvsa.CLI.Scaffolding;

internal static class CliApplication
{
    private const string ForceFlag = "--force";
    private const string BaseEntityFlag = "--base";

    public static int Run(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        try
        {
            var command = args[0].Trim().ToLowerInvariant();

            return command switch
            {
                "createentity" or "make:entity" => RunMakeEntity(args),
                "make:feature" => RunMakeFeature(args),
                _ => UnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static int RunMakeEntity(string[] args)
    {
        EnsureSupportedFlags(args, ForceFlag, BaseEntityFlag);
        var entityName = GetEntityName(args);
        var force = HasFlag(args, ForceFlag);
        var useBaseEntity = HasFlag(args, BaseEntityFlag);

        var project = ProjectLocator.Find(Directory.GetCurrentDirectory());
        EntityScaffolder.Scaffold(project, entityName, force, useBaseEntity);
        return 0;
    }

    private static int RunMakeFeature(string[] args)
    {
        EnsureSupportedFlags(args, ForceFlag);
        var entityName = GetEntityName(args);
        var force = HasFlag(args, ForceFlag);

        var project = ProjectLocator.Find(Directory.GetCurrentDirectory());
        FeatureScaffolder.Scaffold(project, entityName, force);
        return 0;
    }

    private static string GetEntityName(string[] args)
    {
        if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException("Entity name is required.");
        }

        var entityName = args[1].Trim();
        Naming.ValidateEntityName(entityName);
        return entityName;
    }

    private static bool HasFlag(IEnumerable<string> args, string flag) =>
        args.Any(arg => string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase));

    private static void EnsureSupportedFlags(string[] args, params string[] allowedFlags)
    {
        var allowed = allowedFlags.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var argument in args.Skip(2))
        {
            if (!argument.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Unexpected positional argument '{argument}'.");
            }

            if (!allowed.Contains(argument))
            {
                throw new ArgumentException($"Unsupported option '{argument}' for this command.");
            }
        }
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet t3mmyvsa make:entity <EntityName> [--base] [--force]");
        Console.WriteLine("  dotnet t3mmyvsa make:feature <EntityName> [--force]");
        Console.WriteLine();
        Console.WriteLine("Defaults:");
        Console.WriteLine("  make:entity creates an AuditableEntity.");
        Console.WriteLine("  Use --base only when audit timestamps are intentionally unnecessary.");
        Console.WriteLine("  Existing generated files are never overwritten unless --force is supplied.");
    }
}
