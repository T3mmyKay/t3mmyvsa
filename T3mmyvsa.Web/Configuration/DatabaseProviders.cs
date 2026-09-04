namespace T3mmyvsa.Configuration;

public static class DatabaseProviders
{
    public const string SqlServer = "sqlserver";
    public const string PostgreSql = "postgresql";
    public const string MySql = "mysql";
    public const string Sqlite = "sqlite";

    private static readonly Dictionary<string, string> Aliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [SqlServer] = SqlServer,
            ["mssql"] = SqlServer,
            ["sql-server"] = SqlServer,
            [PostgreSql] = PostgreSql,
            ["postgres"] = PostgreSql,
            ["pgsql"] = PostgreSql,
            ["npgsql"] = PostgreSql,
            [MySql] = MySql,
            [Sqlite] = Sqlite,
            ["sqlite3"] = Sqlite
        };

    public static bool TryNormalize(string? value, out string provider)
    {
        if (!string.IsNullOrWhiteSpace(value) &&
            Aliases.TryGetValue(value.Trim(), out var normalized))
        {
            provider = normalized;
            return true;
        }

        provider = string.Empty;
        return false;
    }

    public static string Normalize(string? value)
    {
        if (TryNormalize(value, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException(
            $"Unsupported database provider '{value}'. Supported providers: {SqlServer}, {PostgreSql}, {MySql}, {Sqlite}.");
    }

    public static string BuildTimeConnectionString(string provider) =>
        provider switch
        {
            SqlServer =>
                "Server=localhost;Database=T3mmyVsaBuild;Integrated Security=True;TrustServerCertificate=True",
            PostgreSql =>
                "Host=localhost;Database=t3mmyvsa_build;Username=build",
            MySql =>
                "Server=localhost;Database=t3mmyvsa_build;User=build",
            Sqlite =>
                "Data Source=:memory:",
            _ => throw new InvalidOperationException($"Unsupported database provider '{provider}'.")
        };
}
