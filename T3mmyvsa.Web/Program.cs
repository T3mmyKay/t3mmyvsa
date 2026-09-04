using System.Reflection;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Exceptions;
using T3mmyvsa.Configuration;
using T3mmyvsa.Data;
using T3mmyvsa.Extensions;
using T3mmyvsa.Filters;
using T3mmyvsa.OpenApi;
using T3mmyvsa.Security;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);
    var isBuildTimeOpenApiGeneration =
        string.Equals(
            Assembly.GetEntryAssembly()?.GetName().Name,
            "GetDocument.Insider",
            StringComparison.Ordinal);

    builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

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

    builder.Services.ConfigureDatabaseSettings(builder.Configuration);
    builder.Services.ConfigureSqlContext(builder.Configuration);
    builder.Services.ConfigureDbConnection(builder.Configuration);
    builder.Services.ConfigureIdentity();
    builder.Services.ConfigureJwt(builder.Configuration);
    builder.Services.ConfigureMail(builder.Configuration);
    builder.Services.ConfigureAppSettings(builder.Configuration);
    builder.Services.ConfigureBootstrapAdmin();
    builder.Services.ConfigureServiceScanning();
    builder.Services.ConfigureApiVersioning();
    builder.Services.ConfigureValidation();
    builder.Services.ConfigureAuthorization();
    builder.Services.ConfigureHttpContextAccessor();
    builder.Services.ConfigureProblemDetails();
    builder.Services.ConfigureCarter();
    builder.Services.ConfigureCors(builder.Configuration);
    builder.Services.ConfigureForwardedHeaders(builder.Configuration);
    builder.Services.ConfigureRateLimiting(builder.Configuration);
    builder.Services.ConfigureTransportSecurity();
    builder.Services.ConfigureHangfire(builder.Configuration, !isBuildTimeOpenApiGeneration);
    builder.Services.ConfigureHealthChecks(builder.Configuration);
    builder.Services.ConfigureCortexMediator(builder.Configuration);
    builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    {
        options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

    var app = builder.Build();

    app.UseStatusCodePages(async statusCodeContext =>
    {
        var httpContext = statusCodeContext.HttpContext;
        var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        var statusCode = httpContext.Response.StatusCode;

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode switch
                {
                    StatusCodes.Status400BadRequest => "Bad Request",
                    StatusCodes.Status401Unauthorized => "Unauthorized",
                    StatusCodes.Status403Forbidden => "Forbidden",
                    StatusCodes.Status404NotFound => "Not Found",
                    StatusCodes.Status409Conflict => "Conflict",
                    StatusCodes.Status429TooManyRequests => "Too Many Requests",
                    _ => "Request Failed"
                }
            }
        });
    });
    app.UseExceptionHandler();

    app.UseConfiguredForwardedHeaders();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseSecurityHeaders();
    app.MapStaticAssets();
    app.UseSerilogRequestLogging();

    app.UseRouting();
    app.UseCors(SecurityExtensions.CorsPolicyName);
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    var exposeApiDocs = isBuildTimeOpenApiGeneration ||
                        app.Environment.IsDevelopment() ||
                        app.Configuration.GetValue<bool>("ApiDocumentation:Enabled");
    if (exposeApiDocs)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
    }

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false
    }).AllowAnonymous();

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready")
    }).AllowAnonymous();

    var hangfireSettings = app.Services.GetRequiredService<IOptions<HangfireSettings>>().Value;
    if (!isBuildTimeOpenApiGeneration && hangfireSettings.Enabled && hangfireSettings.Dashboard.Enabled)
    {
        app.UseHangfireDashboard(hangfireSettings.Dashboard.Path, new DashboardOptions
        {
            Authorization = [app.Services.GetRequiredService<HangfireDashboardAuthorizationFilter>()],
            IsReadOnlyFunc = _ => hangfireSettings.Dashboard.ReadOnly
        });
    }

    var versionSet = app.NewApiVersionSet()
        .HasApiVersion(new Asp.Versioning.ApiVersion(1))
        .HasApiVersion(new Asp.Versioning.ApiVersion(2))
        .ReportApiVersions()
        .Build();

    app.MapGroup("api/v{version:apiVersion}")
        .WithApiVersionSet(versionSet)
        .AddEndpointFilter<ValidationFilter>()
        .MapCarter();

    if (!isBuildTimeOpenApiGeneration)
    {
        using var scope = app.Services.CreateScope();
        await DbSeeder.SeedAsync(scope.ServiceProvider);
    }

    await app.RunAsync();
}
catch (Microsoft.Extensions.Hosting.HostAbortedException)
{
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
