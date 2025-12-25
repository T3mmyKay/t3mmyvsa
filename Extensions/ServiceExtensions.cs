using System.Text;
using Cortex.Mediator.DependencyInjection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using T3mmyvsa.Attributes;
using T3mmyvsa.Configuration;
using T3mmyvsa.Data;
using T3mmyvsa.Entities;
using TickerQ.DependencyInjection;
using T3mmyvsa.Interfaces;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.Dashboard.DependencyInjection;

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
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                    if (context.Exception is BadHttpRequestException badRequestEx)
                    {
                        context.ProblemDetails.Title = "Invalid Request";
                        context.ProblemDetails.Status = StatusCodes.Status400BadRequest;
                        context.ProblemDetails.Detail = badRequestEx.Message;
                        context.ProblemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                        context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    }
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
            var jwtSettingsSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettingsSection);

            var jwtSettings = jwtSettingsSection.Get<JwtSettings>()!;

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
                });
        }

        public void ConfigureSqlContext(IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>((sp, opts) =>
            {
                opts.UseSqlServer(configuration.GetConnectionString("sqlConnection"));
                opts.AddInterceptors(sp.GetRequiredService<Interceptors.AuditInterceptor>());
            });
        }

        public void ConfigureMail(IConfiguration configuration)
        {
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
        }

        public void ConfigureServiceScanning()
        {
            services.Scan(scan => scan
                .FromAssemblyOf<IEmailService>()
                // Register Scoped Services
                .AddClasses(classes => classes.WithAttribute<ScopedServiceAttribute>())
                .AsImplementedInterfaces()
                .AsSelf()
                .WithScopedLifetime()
                // Register Singleton Services
                .AddClasses(classes => classes.WithAttribute<SingletonServiceAttribute>())
                .AsImplementedInterfaces()
                .AsSelf()
                .WithSingletonLifetime()
                // Register Transient Services (Explicit Attribute)
                .AddClasses(classes => classes.WithAttribute<TransientServiceAttribute>())
                .AsImplementedInterfaces()
                .AsSelf()
                .WithTransientLifetime()
                // Register Default Services (Ends with "Service") - Exclude decorated services
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
            services.AddValidation();
            services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        }

        public void ConfigureAuthorization()
        {
            services.AddSingleton<IAuthorizationPolicyProvider, Authorization.Providers.CustomAuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, Authorization.Handlers.RoleAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, Authorization.Handlers.PermissionAuthorizationHandler>();

            services.AddAuthorization();
        }

        public void ConfigureCors()
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
        }

        public void ConfigureTickerQ(IConfiguration configuration)
        {
            services.AddTickerQ(options =>
            {
                options.AddDashboard(dashboardOptions =>
                {
                    dashboardOptions.WithBasicAuth("tickeqAdmin", "tickeqAdmin@123");
                });

                options.ConfigureScheduler(scheduler =>
                {
                    scheduler.MaxConcurrency = 8;
                    scheduler.NodeIdentifier = Environment.MachineName;
                });

                options.AddOperationalStore(efOptions =>
                {
                    var connectionString = configuration.GetConnectionString("sqlConnection");

                    efOptions.UseTickerQDbContext<TickerQDbContext>(optionsBuilder =>
                    {
                        optionsBuilder.UseSqlServer(connectionString, cfg =>
                        {
                            cfg.MigrationsAssembly(typeof(Program).Assembly.GetName().Name!);

                            cfg.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                        });
                    }, schema: "ticker");

                    efOptions.SetDbContextPoolSize(34);
                });
            });
        }
    }
}
