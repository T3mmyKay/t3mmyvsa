# T3mmyVSA Template

A reusable .NET 10 backend starter based on Vertical Slice Architecture with ASP.NET Core Identity, granular permissions, EF Core, FluentValidation, Serilog, Hangfire and hardened production defaults.

## Architecture Highlights

- **Framework**: .NET 10 / ASP.NET Core Web API
- **Pattern**: Vertical Slice Architecture
- **Database**: SQL Server 2022 baseline with EF Core 10
- **Authentication**: ASP.NET Core Identity + JWT sessions
- **Authorization**: Identity roles + granular permissions resolved server-side
- **Validation**: FluentValidation
- **Errors**: RFC-style ProblemDetails through one global exception pipeline
- **Background Jobs**: Hangfire 1.8 with first-party SQL Server storage
- **Logging**: Serilog structured logging
- **API Documentation**: OpenAPI + Scalar in development; disabled in production unless explicitly enabled
- **Health**: `/health/live` and `/health/ready`

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server

### Install the template

```bash
dotnet new install ./T3mmyvsa.Web
dotnet new t3mmyvsa -n YourProjectName
```

### Configure development secrets

No database password, JWT signing secret, mail password, bootstrap-admin password, or dashboard credential is committed to the template. Configure secrets with user-secrets or environment variables.

From the generated web project:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:sqlConnection" "<your SQL Server connection string>"
dotnet user-secrets set "JwtSettings:Secret" "<at least 32 random characters>"
dotnet user-secrets set "MailSettings:Password" "<your SMTP password>"
```

Production deployments should use the hosting platform's secret store/environment configuration rather than `appsettings*.json`.

## Bootstrap Administrator

Bootstrap admin provisioning is disabled by default. To create the initial administrator, configure it externally for the first deployment:

```text
BootstrapAdmin__Enabled=true
BootstrapAdmin__Email=admin@example.com
BootstrapAdmin__Password=<strong one-time bootstrap password>
```

The bootstrap account is created with exactly the `Admin` role. After the account exists, disable `BootstrapAdmin__Enabled`; normal user/role management remains authoritative.

## CORS

CORS is explicit-origin only. Wildcard origins are rejected by configuration validation. Configure production frontend origins with indexed environment variables, for example:

```text
Cors__AllowedOrigins__0=https://app.example.com
Cors__AllowedOrigins__1=https://admin.example.com
```

Credentialed CORS requires at least one explicit origin.

## Reverse Proxy / Load Balancer

Forwarded headers are disabled by default and are accepted only from configured trusted proxy IPs.

```text
Proxy__Enabled=true
Proxy__KnownProxies__0=127.0.0.1
Proxy__ForwardLimit=1
```

Set the real reverse-proxy/load-balancer IPs for production. Do not clear the trusted proxy list to accept arbitrary `X-Forwarded-*` headers.

Also override `AllowedHosts` with the actual production hostname(s).

## Authentication Rate Limits

The anonymous authentication surfaces have separate per-client-IP fixed-window policies:

- login: 5/minute
- registration: 5/10 minutes
- forgot/reset password: 5/15 minutes
- refresh token: 30/minute

All limits are configurable under `RateLimiting` and return HTTP 429 ProblemDetails when exceeded.

## Hangfire

TickerQ has been removed from the starter. Hangfire uses first-party `Hangfire.AspNetCore` and `Hangfire.SqlServer` packages with SQL Server's recommended storage settings, bounded automatic retries and configurable worker/queue settings.

Default settings:

```json
{
  "Hangfire": {
    "Enabled": true,
    "ServerEnabled": true,
    "ConnectionStringName": "sqlConnection",
    "SchemaName": "HangFire",
    "Queues": ["critical", "default"],
    "AutomaticRetryAttempts": 3,
    "PrepareSchemaIfNecessary": true,
    "Dashboard": {
      "Enabled": false,
      "Path": "/jobs",
      "RequireHttps": true,
      "ReadOnly": true
    }
  }
}
```

For stronger isolation, point `ConnectionStringName` to a dedicated SQL Server connection such as `hangfireConnection` and grant that database principal access only to the Hangfire schema. Set `ServerEnabled=false` on API instances if a separate worker deployment processes jobs.

### Enqueue a job

```csharp
public sealed class CleanupJob
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // idempotent cleanup work
        return Task.CompletedTask;
    }
}

backgroundJobClient.Enqueue<CleanupJob>(job => job.ExecuteAsync(CancellationToken.None));
```

Background jobs must be idempotent because distributed workers can retry work after failures.

### Hangfire Dashboard

The dashboard is disabled by default. If enabled, credentials must come from external configuration, HTTPS is required by default, the dashboard is read-only by default, and an IP allowlist can be added.

```text
Hangfire__Dashboard__Enabled=true
Hangfire__Dashboard__Username=operations
Hangfire__Dashboard__Password=<long random password>
Hangfire__Dashboard__AllowedIpAddresses__0=203.0.113.10
```

Never pass secrets, access tokens or sensitive personal data as Hangfire job arguments because job arguments are persisted and visible to dashboard operators.

## Health Checks

- `GET /health/live` checks that the process is alive without touching dependencies.
- `GET /health/ready` verifies the application database and, when enabled, Hangfire storage.

Use `/health/ready` for deployment/load-balancer readiness and `/health/live` for liveness monitoring.

## Production Defaults

- no embedded runtime credentials or signing secrets
- Kestrel server header disabled
- HSTS outside Development
- `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, and restrictive `Permissions-Policy`
- OpenAPI/Scalar disabled in production unless `ApiDocumentation:Enabled=true`
- forwarded headers trusted only from configured proxies
- explicit CORS origins
- auth-specific rate limiting
- protected Hangfire dashboard

## CLI Tooling

```bash
dotnet tool install -g T3mmyvsa.CLI
dotnet t3mmyvsa make:entity YourEntityName
dotnet t3mmyvsa make:feature YourEntityName
```

## Updating a locally installed template

```bash
dotnet new install --force ./T3mmyvsa.Web
```

## License

Distributed under the MIT License. See `LICENSE` for more information.
