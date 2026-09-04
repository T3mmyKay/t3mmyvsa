using System.Security.Claims;
using System.Text;
using Cortex.Mediator.DependencyInjection;
using FluentValidation;
using Hangfire;
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
    extension(IServiceCollection services)
    {
        public void ConfigureIdentity()
        {
            services.AddIdentity<User, IdentityRole>(o =>
                {
                    o.Password.RequireDigit = true;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequiredLength = 10;
                    o.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
        }

        public void ConfigureHttpContextAccessor()
        {
            services.AddHttpContextAccessor();
        }

        public void ConfigureProblemDetails()
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                };
            });
        }

        public void ConfigureCarter()
        {
            services.AddCarter();
        }

        public void ConfigureCortexMediator(IConfiguration configuration)
        {
            services.AddCortexMediator(configuration, [typeof(Program)], options => options.AddDefaultBehaviors());
        }

        public void ConfigureJwt(IConfiguration configuration)
        {
            services.AddOptionsWithFluentValidation<JwtSettings>("JwtSettings");
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

            services.AddAuthentication(opt =>
                {
                    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                            var sessionValue = context.Principal?.FindFirstValue(AuthClaimTypes.SessionId);
                            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(sessionValue, out var sessionId))
                            {
                                context.Fail("The access token is not bound to a valid session.");
                                return;
                            }

                            var sessionService = context.HttpContext.RequestServices.GetRequiredService<IAuthSessionService>();
                            if (!await sessionService.IsSessionActiveAsync(userId, sessionId, context.HttpContext.RequestAborted))
                            {
                                context.Fail("The authentication session has been revoked or expired.");
                            }
                        }
                    };
                });
        }

        public void ConfigureSqlContext(IConfiguration configuration)
        {
            var databaseSettings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
            var connectionString = configuration.GetConnectionString("sqlConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:sqlConnection must be supplied through environment configuration, user-secrets, or the deployment secret store.");
            }

            services.AddDbContextFactory<AppDbContext>((sp, opts) =>
            {
                switch (databaseSettings?.DBProvider?.ToLowerInvariant())
                {
                    case "mysql":
                        opts.UseMySQL(connectionString);
                        break;
                    case "mssql":
                    default:
                        opts.UseSqlServer(connectionString);
                        break;
                }

                opts.AddInterceptors(sp.GetRequiredService<Interceptors.AuditInterceptor>());
            });
        }

        public void ConfigureDbConnection(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("sqlConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:sqlConnection must be supplied through environment configuration, user-secrets, or the deployment secret store.");
            }

            services.AddScoped<System.Data.IDbConnection>(_ =>
                new Microsoft.Data.SqlClient.SqlConnection(connectionString));
        }

        public void ConfigureDatabaseSettings(IConfiguration configuration)
        {
            services.AddOptionsWithFluentValidation<DatabaseSettings>("DatabaseSettings");
        }

        public void ConfigureAppSettings(IConfiguration configuration)
        {
            services.AddOptionsWithFluentValidation<AppSettings>("AppSettings");
        }

        public void ConfigureMail(IConfiguration configuration)
        {
            services.AddOptionsWithFluentValidation<MailSettings>("MailSettings");
        }

        public void ConfigureServiceScanning()
        {
            services.Scan(scan => scan
                .FromAssemblyOf<IEmailService>()
                .AddClasses(classes => classes.WithAttribute<ScopedServiceAttribute>())
                .AsImplementedInterfaces()
                .AsSelf()
                .WithScopedLifetime()
                .AddClasses(classes => classes.WithAttribute<SingletonServiceAttribute>())
                .AsImplementedInterfaces()
                .AsSelf()
                .WithSingletonLifetime()
                .AddClasses(classes => classes.WithAttribute<TransientServiceAttribute>())
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

        public void ConfigureApiVersioning()
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

        public void ConfigureValidation()
        {
            services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        }

        public void ConfigureAuthorization()
        {
            services.AddSingleton<IAuthorizationPolicyProvider, Authorization.Providers.CustomAuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, Authorization.Handlers.RoleAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, Authorization.Handlers.PermissionAuthorizationHandler>();
            services.AddAuthorization();
        }

        public void ConfigureHangfire(IConfiguration configuration, bool startServer)
        {
            services.AddOptionsWithFluentValidation<HangfireSettings>("Hangfire");
            var settings = configuration.GetSection("Hangfire").Get<HangfireSettings>() ?? new HangfireSettings();
            if (!settings.Enabled)
            {
                return;
            }

            var connectionString = configuration.GetConnectionString(settings.ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"ConnectionStrings:{settings.ConnectionStringName} must be configured when Hangfire is enabled.");
            }

            services.AddSingleton<HangfireDashboardAuthorizationFilter>();
            services.AddHangfire((_, hangfire) =>
            {
                hangfire
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                    {
                        SchemaName = settings.SchemaName,
                        PrepareSchemaIfNecessary = settings.PrepareSchemaIfNecessary,
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true,
                        TryAutoDetectSchemaDependentOptions = false
                    })
                    .UseFilter(new AutomaticRetryAttribute
                    {
                        Attempts = settings.AutomaticRetryAttempts,
                        OnAttemptsExceeded = AttemptsExceededAction.Fail
                    });
            });

            if (startServer)
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
    }
}
