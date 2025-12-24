using TickerQ.Utilities.Base;
using Serilog;

namespace T3mmyvsa.Features.Jobs;

public class HelloWorldJob
{
    [TickerFunction("HelloWorld")]
    public async Task HelloWorld(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        Log.Information("Hello from TickerQ! Job ID: {JobId}", context.Id);
        Log.Information("Scheduled at: {ScheduledTime}", DateTime.UtcNow);

        await Task.CompletedTask;
    }
}
