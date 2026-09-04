using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace T3mmyvsa.Health;

public sealed class HangfireHealthCheck(JobStorage jobStorage) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = jobStorage.GetMonitoringApi().GetStatistics();
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Hangfire storage is reachable; {statistics.Servers} server(s) registered."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire storage is unreachable.", ex));
        }
    }
}
