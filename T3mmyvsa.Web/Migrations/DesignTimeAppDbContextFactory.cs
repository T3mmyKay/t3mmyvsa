using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using T3mmyvsa.Configuration;
using T3mmyvsa.Data;

namespace T3mmyvsa.Migrations;

public sealed class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var settings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>() ?? new DatabaseSettings();
        var provider = DatabaseProviders.Normalize(settings.ConfiguredProvider);
        var connectionString = ResolveConnectionString(configuration, settings);
        var migrationsAssembly = typeof(DesignTimeAppDbContextFactory).Assembly.GetName().Name!;
        var options = new DbContextOptionsBuilder<AppDbContext>();

        switch (provider)
        {
            case DatabaseProviders.SqlServer:
                options.UseSqlServer(connectionString, database => database.MigrationsAssembly(migrationsAssembly));
                break;
            case DatabaseProviders.PostgreSql:
                options.UseNpgsql(connectionString, database => database.MigrationsAssembly(migrationsAssembly));
                break;
            case DatabaseProviders.MySql:
                options.UseMySQL(connectionString, database => database.MigrationsAssembly(migrationsAssembly));
                break;
            case DatabaseProviders.Sqlite:
                options.UseSqlite(connectionString, database => database.MigrationsAssembly(migrationsAssembly));
                break;
            default:
                throw new InvalidOperationException($"Unsupported database provider '{provider}'.");
        }

        return new AppDbContext(options.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var basePath = ResolveConfigurationBasePath();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string ResolveConnectionString(IConfiguration configuration, DatabaseSettings settings)
    {
        var connectionString = configuration.GetConnectionString(settings.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString) &&
            string.Equals(settings.ConnectionStringName, "appDatabase", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = configuration.GetConnectionString("sqlConnection");
        }

        return !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new InvalidOperationException(
                $"ConnectionStrings:{settings.ConnectionStringName} must be configured before running migrations.");
    }

    private static string ResolveConfigurationBasePath()
    {
        var candidates = new[]
        {
            Directory.GetCurrentDirectory(),
            Directory.GetParent(Directory.GetCurrentDirectory())?.FullName,
            AppContext.BaseDirectory
        };

        foreach (var candidate in candidates.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            if (File.Exists(Path.Combine(candidate!, "appsettings.json")))
            {
                return candidate!;
            }
        }

        throw new InvalidOperationException("Could not locate appsettings.json for migration configuration.");
    }
}
