namespace T3mmyvsa.Features.Jobs;

public sealed class HelloWorldJob(ILogger<HelloWorldJob> logger)
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Hello from Hangfire at {ExecutedAtUtc}.", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }
}
