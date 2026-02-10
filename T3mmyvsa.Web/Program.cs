using Scalar.AspNetCore;
using Serilog;
using Serilog.Exceptions;
using TickerQ.DependencyInjection;
using T3mmyvsa.Data;
using T3mmyvsa.Extensions;
using T3mmyvsa.Filters;
using T3mmyvsa.OpenApi;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails());

    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        options.AddDocumentTransformer<ServerUrlTransformer>();
    });
    builder.Services.AddOpenApi("v2", options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        options.AddDocumentTransformer<ServerUrlTransformer>();
    });

    builder.Services.ConfigureSqlContext(builder.Configuration);
    builder.Services.ConfigureDbConnection(builder.Configuration);
    builder.Services.ConfigureIdentity();
    builder.Services.ConfigureJwt(builder.Configuration);
    builder.Services.ConfigureMail(builder.Configuration);
    builder.Services.ConfigureAppSettings(builder.Configuration);
    builder.Services.ConfigureServiceScanning();
    builder.Services.ConfigureApiVersioning();
    builder.Services.ConfigureValidation();
    builder.Services.ConfigureAuthorization();
    builder.Services.ConfigureHttpContextAccessor();
    builder.Services.ConfigureProblemDetails();
    builder.Services.ConfigureCarter();
    builder.Services.ConfigureTickerQ(builder.Configuration);
    builder.Services.ConfigureCortexMediator(builder.Configuration);
    builder.Services.ConfigureCors();
    builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    {
        options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });


    var app = builder.Build();

    // Configure the HTTP request pipeline.
    // Configure the HTTP request pipeline.
    app.UseStatusCodePages();
    app.UseExceptionHandler();

    app.UseHttpsRedirection();
    app.MapStaticAssets();

    app.UseSerilogRequestLogging();

    app.UseCors("CorsPolicy");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

    app.UseTickerQ();

    var versionSet = app.NewApiVersionSet()
        .HasApiVersion(new Asp.Versioning.ApiVersion(1))
        .HasApiVersion(new Asp.Versioning.ApiVersion(2))
        .ReportApiVersions()
        .Build();

    app.MapGroup("api/v{version:apiVersion}")
        .WithApiVersionSet(versionSet)
        .AddEndpointFilter<ValidationFilter>()
        .MapCarter();

    using (var scope = app.Services.CreateScope())
    {
        await DbSeeder.SeedAsync(scope.ServiceProvider);
    }

    await app.RunAsync();
}
catch (Microsoft.Extensions.Hosting.HostAbortedException)
{
    // Ignore this exception as it is thrown by EF Core tools when generating migrations
    Log.Information("Host aborted (likely by EF Core tools).");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
