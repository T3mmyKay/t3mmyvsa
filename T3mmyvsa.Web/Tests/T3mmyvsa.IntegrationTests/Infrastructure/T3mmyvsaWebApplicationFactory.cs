using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using T3mmyvsa.Data;

namespace T3mmyvsa.IntegrationTests.Infrastructure;

public sealed class T3mmyvsaWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly string[] TestEnvironmentKeys =
    [
        "ASPNETCORE_ENVIRONMENT",
        "AllowedHosts",
        "DatabaseSettings__Provider",
        "DatabaseSettings__ConnectionStringName",
        "ConnectionStrings__appDatabase",
        "JwtSettings__Secret",
        "Hangfire__Enabled",
        "BootstrapAdmin__Enabled",
        "ApiDocumentation__Enabled"
    ];

    private readonly Dictionary<string, string?> _originalEnvironment = new(StringComparer.Ordinal);
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("t3mmyvsa_integration")
        .WithUsername("postgres")
        .WithPassword("integration-test-password")
        .WithAutoRemove(true)
        .WithCleanUp(true)
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync(TestContext.Current.CancellationToken);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        }

        SetTestEnvironment();
        _ = Server;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await base.DisposeAsync();
            await _postgres.DisposeAsync();
        }
        finally
        {
            foreach (var (key, value) in _originalEnvironment)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private void SetTestEnvironment()
    {
        foreach (var key in TestEnvironmentKeys)
        {
            _originalEnvironment[key] = Environment.GetEnvironmentVariable(key);
        }

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("AllowedHosts", "localhost");
        Environment.SetEnvironmentVariable("DatabaseSettings__Provider", "postgresql");
        Environment.SetEnvironmentVariable("DatabaseSettings__ConnectionStringName", "appDatabase");
        Environment.SetEnvironmentVariable("ConnectionStrings__appDatabase", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable(
            "JwtSettings__Secret",
            "IntegrationOnlyJwtSigningSecret_12345678901234567890");
        Environment.SetEnvironmentVariable("Hangfire__Enabled", "false");
        Environment.SetEnvironmentVariable("BootstrapAdmin__Enabled", "false");
        Environment.SetEnvironmentVariable("ApiDocumentation__Enabled", "false");
    }
}
