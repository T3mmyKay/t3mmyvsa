using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;
using T3mmyvsa.Extensions;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // A worker deployment exists to process jobs. The API's default stays enqueue-only.
    builder.Configuration["Hangfire:ServerEnabled"] = "true";
    builder.Configuration["Hangfire:Dashboard:Enabled"] = "false";

    builder.Services.AddSerilog((services, configuration) =>
        configuration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails());

    builder.Services.ConfigureHttpContextAccessor();
    builder.Services.ConfigureServiceScanning();
    builder.Services.ConfigureDatabase(builder.Configuration);
    builder.Services.ConfigureIdentity();
    builder.Services.ConfigureMail(builder.Configuration);
    builder.Services.ConfigureAppSettings(builder.Configuration);
    builder.Services.ConfigureValidation();
    builder.Services.ConfigureAuthorization();
    builder.Services.ConfigureCortexMediator(builder.Configuration);
    builder.Services.ConfigureHangfire(builder.Configuration, startServer: true);

    var host = builder.Build();
    Log.Information("Starting T3mmyVSA background worker");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Background worker terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
