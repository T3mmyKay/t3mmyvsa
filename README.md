# T3mmyVSA Template

A reusable .NET 10 backend starter based on Vertical Slice Architecture with ASP.NET Core Identity, granular permissions, multi-provider EF Core persistence, FluentValidation, Serilog, Hangfire, health checks and hardened production defaults.

## Architecture Highlights

- **Framework**: .NET 10 / ASP.NET Core Web API
- **Pattern**: Vertical Slice Architecture
- **Database**: EF Core with SQL Server, PostgreSQL, MySQL and SQLite
- **Authentication**: ASP.NET Core Identity + JWT sessions
- **Authorization**: Identity roles + granular permissions resolved server-side
- **Validation**: FluentValidation
- **Errors**: RFC-style ProblemDetails through one global exception pipeline
- **Background Jobs**: Hangfire with SQL Server or PostgreSQL storage
- **Logging**: Serilog structured logging
- **API Documentation**: OpenAPI + Scalar in development; disabled in production unless explicitly enabled
- **Health**: `/health/live` and `/health/ready`

## Getting Started

### Prerequisites

- .NET 10 SDK
- one supported application database:
  - SQL Server
  - PostgreSQL
  - MySQL
  - SQLite

### Install the template

```bash
dotnet new install ./T3mmyvsa.Web
dotnet new t3mmyvsa -n YourProjectName
```

### Configure the database

The application database provider is selected at startup.

```text
DatabaseSettings__Provider=postgresql
DatabaseSettings__ConnectionStringName=appDatabase
ConnectionStrings__appDatabase=Host=localhost;Database=myapp;Username=postgres;Password=<secret>
```

Canonical provider values are:

| Provider | Value | Accepted aliases |
| --- | --- | --- |
| SQL Server | `sqlserver` | `mssql`, `sql-server` |
| PostgreSQL | `postgresql` | `postgres`, `pgsql`, `npgsql` |
| MySQL | `mysql` | — |
| SQLite | `sqlite` | `sqlite3` |

SQL Server remains the default when no provider is specified. `DBProvider` and `ConnectionStrings:sqlConnection` are accepted as compatibility aliases for pre-v2 projects, but new projects should use `Provider` and `appDatabase`.

Examples:

```text
# SQL Server
DatabaseSettings__Provider=sqlserver
ConnectionStrings__appDatabase=Server=localhost;Database=myapp;User Id=sa;Password=<secret>;TrustServerCertificate=True

# PostgreSQL
DatabaseSettings__Provider=postgresql
ConnectionStrings__appDatabase=Host=localhost;Database=myapp;Username=postgres;Password=<secret>

# MySQL
DatabaseSettings__Provider=mysql
ConnectionStrings__appDatabase=Server=localhost;Database=myapp;User=root;Password=<secret>

# SQLite
DatabaseSettings__Provider=sqlite
ConnectionStrings__appDatabase=Data Source=app.db
```

See `docs/DATABASE_PROVIDERS.md` in generated projects for migration and provider-switching rules.

### Generate the initial migration

The template intentionally does **not** ship SQL Server-generated migration files into new applications. Choose the provider first, configure its connection string, then generate the migration with that provider active:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

EF migrations are provider-specific. Do not generate migrations with SQL Server and then attempt to apply the same migration history to PostgreSQL, MySQL or SQLite.

### Configure development secrets

No database password, JWT signing secret, mail password, bootstrap-admin password or Hangfire dashboard credential is committed to the template.

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:appDatabase" "<your database connection string>"
dotnet user-secrets set "JwtSettings:Secret" "<at least 32 random characters>"
dotnet user-secrets set "MailSettings:Password" "<your SMTP password>"
```

Production deployments should use the hosting platform's secret store/environment configuration.

## Bootstrap Administrator

Bootstrap admin provisioning is disabled by default.

```text
BootstrapAdmin__Enabled=true
BootstrapAdmin__Email=admin@example.com
BootstrapAdmin__Password=<strong one-time bootstrap password>
```

The bootstrap account is created with exactly the `Admin` role. Disable `BootstrapAdmin__Enabled` after provisioning.

## CORS

CORS is explicit-origin only.

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

Also override `AllowedHosts` with the real production hostname(s).

## Authentication Rate Limits

The anonymous authentication surfaces have separate per-client-IP policies:

- login: 5/minute
- registration: 5/10 minutes
- forgot/reset password: 5/15 minutes
- refresh token: 30/minute

All limits are configurable under `RateLimiting` and return HTTP 429 ProblemDetails when exceeded.

## Hangfire

Hangfire storage is intentionally independent from the application database.

Supported production storage providers:

- `sqlserver`
- `postgresql`

Default configuration:

```json
{
  "Hangfire": {
    "Enabled": true,
    "ServerEnabled": true,
    "StorageProvider": "inherit",
    "ConnectionStringName": "",
    "SchemaName": "hangfire",
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

`StorageProvider=inherit` means:

- SQL Server app → Hangfire uses SQL Server
- PostgreSQL app → Hangfire uses PostgreSQL
- MySQL/SQLite app → startup fails if Hangfire remains enabled

For MySQL or SQLite applications, either disable Hangfire:

```text
Hangfire__Enabled=false
```

or point Hangfire to a separate SQL Server/PostgreSQL database:

```text
Hangfire__StorageProvider=postgresql
Hangfire__ConnectionStringName=hangfireDatabase
ConnectionStrings__hangfireDatabase=Host=localhost;Database=jobs;Username=postgres;Password=<secret>
```

This avoids pretending every community Hangfire storage provider has the same production guarantees.

Set `Hangfire__ServerEnabled=false` on API instances if a dedicated worker deployment processes jobs.

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

backgroundJobClient.Enqueue<CleanupJob>(
    job => job.ExecuteAsync(CancellationToken.None));
```

Background jobs must be idempotent because distributed workers can retry work.

### Hangfire Dashboard

The dashboard is disabled by default. If enabled, credentials come from external configuration, HTTPS is required by default, the dashboard is read-only by default, and an IP allowlist can be added.

```text
Hangfire__Dashboard__Enabled=true
Hangfire__Dashboard__Username=operations
Hangfire__Dashboard__Password=<long random password>
Hangfire__Dashboard__AllowedIpAddresses__0=203.0.113.10
```

Never pass secrets, access tokens or unnecessary personal data as Hangfire job arguments.

## Health Checks

- `GET /health/live` checks only process liveness.
- `GET /health/ready` checks the selected application database and, when enabled, Hangfire storage.

The application database health check is EF-provider agnostic.

## Production Defaults

- no embedded runtime credentials or signing secrets
- Kestrel server header disabled
- HSTS outside Development
- hardened security headers
- OpenAPI/Scalar disabled in production unless explicitly enabled
- forwarded headers trusted only from configured proxies
- explicit CORS origins
- auth-specific rate limiting
- protected Hangfire dashboard

## CLI Tooling

Install the companion tool explicitly:

```bash
dotnet tool install -g T3mmyvsa.CLI
```

Create an auditable entity and CRUD slices:

```bash
dotnet t3mmyvsa make:entity Product
dotnet t3mmyvsa make:feature Product
```

Useful options:

```bash
dotnet t3mmyvsa make:entity LookupValue --base
dotnet t3mmyvsa make:feature Product --force
```

The CLI uses `Guid` identifiers, FluentValidation, granular View/Create/Update/Delete permissions, bounded pagination, deterministic sorting, ProblemDetails-compatible exceptions, cancellation propagation and the starter's API versioning conventions.

## Updating a locally installed template

```bash
dotnet new install --force ./T3mmyvsa.Web
```

## License

Distributed under the MIT License. See `LICENSE` for more information.
