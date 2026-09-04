using Microsoft.EntityFrameworkCore.Design;
using T3mmyvsa.Configuration;

namespace T3mmyvsa.Data;

public sealed class DesignTimeAppDbContextFactory
    : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = ResolveConfigurationBasePath();
        var environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile(
                $"appsettings.{environment}.json",
                optional: true)
            .AddEnvironmentVariables()
            .Build();

        var settings =
            configuration.GetSection("DatabaseSettings")
                .Get<DatabaseSettings>()
            ?? new DatabaseSettings();

        var provider =
            DatabaseProviders.Normalize(settings.ConfiguredProvider);

        var connectionString =
            configuration.GetConnectionString(settings.ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString) &&
            string.Equals(
                settings.ConnectionStringName,
                "appDatabase",
                StringComparison.OrdinalIgnoreCase))
        {
            connectionString =
                configuration.GetConnectionString("sqlConnection");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"ConnectionStrings:{settings.ConnectionStringName} must be configured before running EF Core design-time commands.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>();

        switch (provider)
        {
            case DatabaseProviders.SqlServer:
                options.UseSqlServer(connectionString);
                break;

            case DatabaseProviders.PostgreSql:
                options.UseNpgsql(connectionString);
                break;

            case DatabaseProviders.MySql:
                options.UseMySQL(connectionString);
                break;

            case DatabaseProviders.Sqlite:
                options.UseSqlite(connectionString);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported database provider '{provider}'.");
        }

        return new AppDbContext(options.Options);
    }

    private static string ResolveConfigurationBasePath()
    {
        var current = Directory.GetCurrentDirectory();
        if (File.Exists(Path.Combine(current, "appsettings.json")))
        {
            return current;
        }

        var webProject = Path.Combine(current, "T3mmyvsa.Web");
        if (File.Exists(Path.Combine(webProject, "appsettings.json")))
        {
            return webProject;
        }

        throw new InvalidOperationException(
            "Could not locate appsettings.json for EF Core design-time configuration.");
    }
}
