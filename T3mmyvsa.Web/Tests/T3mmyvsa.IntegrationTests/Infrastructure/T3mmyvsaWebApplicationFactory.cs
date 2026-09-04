using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using T3mmyvsa.Data;

namespace T3mmyvsa.IntegrationTests.Infrastructure;

public sealed class T3mmyvsaWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("t3mmyvsa_integration")
        .WithUsername("postgres")
        .WithPassword("integration-test-password")
        .WithAutoRemove(true)
        .WithCleanUp(true)
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
        }

        _ = Server;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AllowedHosts"] = "localhost",
                ["DatabaseSettings:Provider"] = "postgresql",
                ["DatabaseSettings:ConnectionStringName"] = "appDatabase",
                ["ConnectionStrings:appDatabase"] = _postgres.GetConnectionString(),
                ["JwtSettings:Secret"] = "IntegrationOnlyJwtSigningSecret_12345678901234567890",
                ["Hangfire:Enabled"] = "false",
                ["BootstrapAdmin:Enabled"] = "false",
                ["ApiDocumentation:Enabled"] = "false"
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
