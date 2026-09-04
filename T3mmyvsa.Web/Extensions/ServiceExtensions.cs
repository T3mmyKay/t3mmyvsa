using System.Security.Claims;
using System.Text;
using Cortex.Mediator.DependencyInjection;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using T3mmyvsa.Attributes;
using T3mmyvsa.Authorization;
using T3mmyvsa.Configuration;
using T3mmyvsa.Data;
using T3mmyvsa.Entities;
using T3mmyvsa.Exceptions;
using T3mmyvsa.Interfaces;
using T3mmyvsa.Security;

namespace T3mmyvsa.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 10;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    }

    public static void ConfigureHttpContextAccessor(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
    }

    public static void ConfigureProblemDetails(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions.TryAdd(
                    "requestId",
                    context.HttpContext.TraceIdentifier);
            };
        });
    }

    public static void ConfigureCarter(this IServiceCollection services)
    {
        services.AddCarter();
    }

    public static void ConfigureCortexMediator(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCortexMediator(
            configuration,
            [typeof(Program)],
            options => options.AddDefaultBehaviors());
    }

    public static void ConfigureJwt(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptionsWithFluentValidation<JwtSettings>("JwtSettings");
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidAudience = jwtSettings.ValidAudience,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userId =
                            context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var sessionValue =
                            context.Principal?.FindFirstValue(AuthClaimTypes.SessionId);

                        if (string.IsNullOrWhiteSpace(userId) ||
                            !Guid.TryParse(sessionValue, out var sessionId))
                        {
                            context.Fail(
                                "The access token is not bound to a valid session.");
                            return;
                        }

                        var sessionService = context.HttpContext.RequestServices
                            .GetRequiredService<IAuthSessionService>();

                        if (!await sessionService.IsSessionActiveAsync(
                                userId,
                                sessionId,
                                context.HttpContext.RequestAborted))
                        {
                            context.Fail(
                                "The authentication session has been revoked or expired.");
                        }
                    }
                };
            });
    }

    public static void ConfigureDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        bool allowBuildTimePlaceholder = false)
    {
        services.AddOptionsWithFluentValidation<DatabaseSettings>("DatabaseSettings");

        var settings =
            configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
            ?? new DatabaseSettings();

        var provider = DatabaseProviders.Normalize(settings.ConfiguredProvider);
        var connectionString = ResolveApplicationConnectionString(
            configuration,
            settings,
            provider,
            allowBuildTimePlaceholder);

        services.AddDbContextFactory<AppDbContext>((serviceProvider, options) =>
        {
            switch (provider)
            {
                case DatabaseProviders.SqlServer:
                    options.UseSqlServer(
                        connectionString,
                        sqlServer => sqlServer.EnableRetryOnFailure());
                    break;

                case DatabaseProviders.PostgreSql:
                    options.UseNpgsql(
                        connectionString,
                        postgreSql => postgreSql.EnableRetryOnFailure());
                    break;

                case DatabaseProviders.MySql:
                    options.UseMySQL(connectionString);
                    break;

                case DatabaseProviders.Sqlite:
                    options.UseSqlite(connectionString);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported database provider '{provider}'.");
            }

            options.AddInterceptors(
                serviceProvider.GetRequiredService<Interceptors.AuditInterceptor>());
        });
    }

    public static void ConfigureDatabaseSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptionsWithFluentValidation<DatabaseSettings>("DatabaseSettings");
    }

    public static void ConfigureAppSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptionsWithFluentValidation<AppSettings>("AppSettings");
    }

    public static void ConfigureMail(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptionsWithFluentValidation<MailSettings>("MailSettings");
    }

    public static void ConfigureServiceScanning(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<IEmailService>()
            .AddClasses(classes =>
                classes.WithAttribute<ScopedServiceAttribute>())
            .AsImplementedInterfaces()
            .AsSelf()
            .WithScopedLifetime()
            .AddClasses(classes =>
                classes.WithAttribute<SingletonServiceAttribute>())
            .AsImplementedInterfaces()
            .AsSelf()
            .WithSingletonLifetime()
            .AddClasses(classes =>
                classes.WithAttribute<TransientServiceAttribute>())
            .AsImplementedInterfaces()
            .AsSelf()
            .WithTransientLifetime()
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("Service") &&
                !type.IsDefined(typeof(ScopedServiceAttribute), false) &&
                !type.IsDefined(typeof(SingletonServiceAttribute), false) &&
                !type.IsDefined(typeof(TransientServiceAttribute), false)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());
    }

    public static void ConfigureApiVersioning(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
                    new Asp.Versioning.UrlSegmentApiVersionReader(),
                    new Asp.Versioning.HeaderApiVersionReader("X-Api-Version"));
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            })
            .EnableApiVersionBinding();
    }

    public static void ConfigureValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    }

    public static void ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<
            IAuthorizationPolicyProvider,
            Authorization.Providers.CustomAuthorizationPolicyProvider>();
        services.AddSingleton<
            IAuthorizationHandler,
            Authorization.Handlers.RoleAuthorizationHandler>();
        services.AddSingleton<
            IAuthorizationHandler,
            Authorization.Handlers.PermissionAuthorizationHandler>();
        services.AddAuthorization();
    }

    public static void ConfigureHangfire(
        this IServiceCollection services,
        IConfiguration configuration,
        bool startServer,
        bool skipStorageConfiguration = false)
    {
        services.AddOptionsWithFluentValidation<HangfireSettings>("Hangfire");

        if (skipStorageConfiguration)
        {
            return;
        }

        var settings =
            configuration.GetSection("Hangfire").Get<HangfireSettings>()
            ?? new HangfireSettings();

        if (!settings.Enabled)
        {
            return;
        }

        var databaseSettings =
            configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
            ?? new DatabaseSettings();

        var applicationProvider =
            DatabaseProviders.Normalize(databaseSettings.ConfiguredProvider);

        var storageProvider = ResolveHangfireStorageProvider(
            settings.StorageProvider,
            applicationProvider);

        var connectionStringName =
            string.IsNullOrWhiteSpace(settings.ConnectionStringName)
                ? databaseSettings.ConnectionStringName
                : settings.ConnectionStringName.Trim();

        var connectionString = ResolveConnectionString(
            configuration,
            connectionStringName,
            allowLegacySqlConnectionFallback:
                string.IsNullOrWhiteSpace(settings.ConnectionStringName));

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"ConnectionStrings:{connectionStringName} must be configured when Hangfire is enabled.");
        }

        services.AddSingleton<HangfireDashboardAuthorizationFilter>();
        services.AddHangfire((_, hangfire) =>
        {
            hangfire
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            switch (storageProvider)
            {
                case DatabaseProviders.SqlServer:
                    hangfire.UseSqlServerStorage(
                        connectionString,
                        new SqlServerStorageOptions
                        {
                            SchemaName = settings.SchemaName,
                            PrepareSchemaIfNecessary =
                                settings.PrepareSchemaIfNecessary,
                            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                            QueuePollInterval = TimeSpan.Zero,
                            UseRecommendedIsolationLevel = true,
                            DisableGlobalLocks = true,
                            TryAutoDetectSchemaDependentOptions = false
                        });
                    break;

                case DatabaseProviders.PostgreSql:
                    hangfire.UsePostgreSqlStorage(
                        options => options.UseNpgsqlConnection(connectionString),
                        new PostgreSqlStorageOptions
                        {
                            SchemaName = settings.SchemaName,
                            PrepareSchemaIfNecessary =
                                settings.PrepareSchemaIfNecessary,
                            StartupConnectionMaxRetries = 5,
                            StartupConnectionBaseDelay = TimeSpan.FromSeconds(1),
                            StartupConnectionMaxDelay = TimeSpan.FromSeconds(30),
                            AllowDegradedModeWithoutStorage = false
                        });
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported Hangfire storage provider '{storageProvider}'.");
            }

            hangfire.UseFilter(new AutomaticRetryAttribute
            {
                Attempts = settings.AutomaticRetryAttempts,
                OnAttemptsExceeded = AttemptsExceededAction.Fail
            });
        });

        if (startServer && settings.ServerEnabled)
        {
            services.AddHangfireServer(options =>
            {
                options.Queues = settings.Queues;
                if (settings.WorkerCount.HasValue)
                {
                    options.WorkerCount = settings.WorkerCount.Value;
                }
            });
        }
    }

    private static string ResolveApplicationConnectionString(
        IConfiguration configuration,
        DatabaseSettings settings,
        string provider,
        bool allowBuildTimePlaceholder)
    {
        var connectionString = ResolveConnectionString(
            configuration,
            settings.ConnectionStringName,
            allowLegacySqlConnectionFallback: true);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        if (allowBuildTimePlaceholder)
        {
            return DatabaseProviders.BuildTimeConnectionString(provider);
        }

        throw new InvalidOperationException(
            $"ConnectionStrings:{settings.ConnectionStringName} must be supplied through environment configuration, user-secrets, or the deployment secret store.");
    }

    private static string? ResolveConnectionString(
        IConfiguration configuration,
        string connectionStringName,
        bool allowLegacySqlConnectionFallback)
    {
        var connectionString =
            configuration.GetConnectionString(connectionStringName);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        if (allowLegacySqlConnectionFallback &&
            string.Equals(
                connectionStringName,
                "appDatabase",
                StringComparison.OrdinalIgnoreCase))
        {
            return configuration.GetConnectionString("sqlConnection");
        }

        return null;
    }

    private static string ResolveHangfireStorageProvider(
        string? configuredStorageProvider,
        string applicationProvider)
    {
        if (string.IsNullOrWhiteSpace(configuredStorageProvider) ||
            string.Equals(
                configuredStorageProvider,
                "inherit",
                StringComparison.OrdinalIgnoreCase))
        {
            if (applicationProvider is DatabaseProviders.SqlServer or
                DatabaseProviders.PostgreSql)
            {
                return applicationProvider;
            }

            throw new InvalidOperationException(
                $"Hangfire cannot inherit the '{applicationProvider}' application database. " +
                $"Set Hangfire:Enabled=false or configure Hangfire:StorageProvider as " +
                $"'{DatabaseProviders.SqlServer}' or '{DatabaseProviders.PostgreSql}' with a supported connection string.");
        }

        var storageProvider =
            DatabaseProviders.Normalize(configuredStorageProvider);

        if (storageProvider is not DatabaseProviders.SqlServer and
            not DatabaseProviders.PostgreSql)
        {
            throw new InvalidOperationException(
                $"Hangfire storage provider '{configuredStorageProvider}' is not supported. " +
                $"Supported production storage providers are '{DatabaseProviders.SqlServer}' and '{DatabaseProviders.PostgreSql}'.");
        }

        return storageProvider;
    }
}
