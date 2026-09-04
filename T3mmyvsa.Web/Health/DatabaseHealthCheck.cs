using Microsoft.Extensions.Diagnostics.HealthChecks;
using T3mmyvsa.Data;

namespace T3mmyvsa.Health;

public sealed class DatabaseHealthCheck(IDbContextFactory<AppDbContext> dbContextFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Application database is reachable.")
                : HealthCheckResult.Unhealthy("Application database is unreachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Application database health check failed.", ex);
        }
    }
}
