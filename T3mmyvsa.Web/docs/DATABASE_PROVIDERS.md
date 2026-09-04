# Database Provider Guide

T3mmyVSA separates the application database provider from Hangfire storage and owns EF migrations in a dedicated project.

## Application database providers

| Canonical value | Provider package |
| --- | --- |
| `sqlserver` | `Microsoft.EntityFrameworkCore.SqlServer` |
| `postgresql` | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| `mysql` | `MySql.EntityFrameworkCore` |
| `sqlite` | `Microsoft.EntityFrameworkCore.Sqlite` |

Configure:

```text
DatabaseSettings__Provider=postgresql
DatabaseSettings__ConnectionStringName=appDatabase
ConnectionStrings__appDatabase=Host=localhost;Database=myapp;Username=postgres;Password=<secret>
```

Aliases: SQL Server `mssql`, `sql-server`; PostgreSQL `postgres`, `pgsql`, `npgsql`; SQLite `sqlite3`. Pre-v2 `DBProvider`/`sqlConnection` remain compatibility aliases.

## Dedicated provider-specific migrations

EF migrations are provider-specific. New projects ship without generated migration history. Generate into `Migrations/Data/Migrations`:

```bash
dotnet ef migrations add InitialCreate \
  --project Migrations/T3mmyvsa.Migrations.csproj \
  --startup-project Migrations/T3mmyvsa.Migrations.csproj \
  --output-dir Data/Migrations
```

The migrations project contains the design-time `AppDbContext` factory and sets itself as the migrations assembly for the selected provider. `dotnet ef` therefore does not execute the web host or start Hangfire.

At deployment, run the compiled migration executable:

```bash
dotnet Migrations/bin/Release/net10.0/T3mmyvsa.Migrations.dll
```

The Docker `migrate` service packages that executable in an ASP.NET runtime image and applies it before API/Worker startup. It retries database startup failures with bounded attempts and fails when no migrations exist.

### Switching providers

Do not reuse migration history across providers. Export/backup data, create a fresh migration history for the new provider, provision/migrate data deliberately and validate behavior before cutover.

## Hangfire storage

Supported production Hangfire storage providers:

- SQL Server
- PostgreSQL

`Hangfire__StorageProvider=inherit` follows the application provider only for SQL Server/PostgreSQL. MySQL/SQLite apps must disable Hangfire or point it to a separate supported store.

The API defaults to `Hangfire__ServerEnabled=false`; the dedicated Worker forces server mode. Storage/client registration remains in the API so it can enqueue jobs.

## Runtime behavior

Normal runtime fails closed when required connection strings are missing. Build-time OpenAPI generation may use a non-network placeholder solely to construct the EF model. Readiness uses `AppDbContext.Database.CanConnectAsync()` and follows the selected provider.
