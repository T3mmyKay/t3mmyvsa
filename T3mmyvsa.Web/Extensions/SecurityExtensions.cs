using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using T3mmyvsa.Configuration;
using T3mmyvsa.Health;

namespace T3mmyvsa.Extensions;

public static class SecurityExtensions
{
    public const string CorsPolicyName = "ConfiguredCorsPolicy";

    public static void ConfigureBootstrapAdmin(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<BootstrapAdminSettings>("BootstrapAdmin");
    }

    public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithFluentValidation<CorsSettings>("Cors");
        var settings = configuration.GetSection("Cors").Get<CorsSettings>() ?? new CorsSettings();

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                if (settings.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(settings.AllowedOrigins);
                }

                policy.WithMethods(settings.AllowedMethods)
                    .WithHeaders(settings.AllowedHeaders)
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(settings.PreflightMaxAgeSeconds));

                if (settings.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });
    }

    public static void ConfigureForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithFluentValidation<ProxySettings>("Proxy");
        var settings = configuration.GetSection("Proxy").Get<ProxySettings>() ?? new ProxySettings();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = settings.ForwardLimit;
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();

            foreach (var value in settings.KnownProxies)
            {
                if (IPAddress.TryParse(value, out var proxy))
                {
                    options.KnownProxies.Add(proxy);
                }
            }
        });
    }

    public static void ConfigureRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithFluentValidation<RateLimitSettings>("RateLimiting");
        var settings = configuration.GetSection("RateLimiting").Get<RateLimitSettings>() ?? new RateLimitSettings();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers["Retry-After"] =
                        Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too Many Requests",
                        Detail = "Too many authentication requests. Try again later."
                    }
                });
            };

            options.AddPolicy(RateLimitPolicyNames.Login,
                httpContext => CreatePartition(httpContext, settings.Login));
            options.AddPolicy(RateLimitPolicyNames.Registration,
                httpContext => CreatePartition(httpContext, settings.Registration));
            options.AddPolicy(RateLimitPolicyNames.Recovery,
                httpContext => CreatePartition(httpContext, settings.Recovery));
            options.AddPolicy(RateLimitPolicyNames.Refresh,
                httpContext => CreatePartition(httpContext, settings.Refresh));
        });
    }

    public static void ConfigureTransportSecurity(this IServiceCollection services)
    {
        services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(180);
            options.IncludeSubDomains = false;
            options.Preload = false;
        });
    }

    public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var builder = services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        var hangfire = configuration.GetSection("Hangfire").Get<HangfireSettings>() ?? new HangfireSettings();
        if (hangfire.Enabled)
        {
            builder.AddCheck<HangfireHealthCheck>("hangfire", tags: ["ready"]);
        }
    }

    public static void UseConfiguredForwardedHeaders(this WebApplication app)
    {
        var settings = app.Services.GetRequiredService<IOptions<ProxySettings>>().Value;
        if (settings.Enabled)
        {
            app.UseForwardedHeaders();
        }
    }

    public static void UseSecurityHeaders(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["Referrer-Policy"] = "no-referrer";
                context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                return Task.CompletedTask;
            });

            await next();
        });
    }

    private static RateLimitPartition<string> CreatePartition(
        HttpContext httpContext,
        RateLimitPolicySettings settings)
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = settings.PermitLimit,
                Window = TimeSpan.FromSeconds(settings.WindowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }
}
