# Database Provider Guide

T3mmyVSA separates the **application database provider** from the **Hangfire storage provider**.

## Application database providers

Supported EF Core providers:

| Canonical value | Provider package |
| --- | --- |
| `sqlserver` | `Microsoft.EntityFrameworkCore.SqlServer` |
| `postgresql` | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| `mysql` | `MySql.EntityFrameworkCore` |
| `sqlite` | `Microsoft.EntityFrameworkCore.Sqlite` |

Configure the provider and the connection-string key:

```text
DatabaseSettings__Provider=postgresql
DatabaseSettings__ConnectionStringName=appDatabase
ConnectionStrings__appDatabase=Host=localhost;Database=myapp;Username=postgres;Password=<secret>
```

Accepted compatibility aliases:

- SQL Server: `mssql`, `sql-server`
- PostgreSQL: `postgres`, `pgsql`, `npgsql`
- SQLite: `sqlite3`

Pre-v2 `DatabaseSettings:DBProvider` and `ConnectionStrings:sqlConnection` are accepted for compatibility, but new applications should use the canonical settings.

## Provider-specific migrations

EF Core migrations are not portable database DDL.

The project template therefore starts **without pre-generated migration files**. After choosing the application provider:

1. configure `DatabaseSettings:Provider`;
2. configure the selected connection string;
3. generate the initial migration;
4. inspect it;
5. apply it.

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

The design-time `AppDbContext` factory reads the same provider settings as runtime, but does not start Hangfire or other application infrastructure.

### Switching providers later

Do not reuse the existing migration history against a different provider.

For a provider switch:

1. back up/export application data;
2. configure the new provider and connection string;
3. create a fresh migration history for the new database;
4. provision the new database;
5. migrate data deliberately;
6. validate provider-specific behavior before cutover.

Keep migrations for one deployed application on one provider line.

## Hangfire storage

Supported production Hangfire storage providers are:

- SQL Server (`Hangfire.SqlServer`)
- PostgreSQL (`Hangfire.PostgreSql`)

By default:

```text
Hangfire__StorageProvider=inherit
```

Inheritance is allowed only when the application database is SQL Server or PostgreSQL.

For a MySQL or SQLite application, either disable Hangfire:

```text
Hangfire__Enabled=false
```

or use a separate supported job database:

```text
Hangfire__StorageProvider=postgresql
Hangfire__ConnectionStringName=hangfireDatabase
ConnectionStrings__hangfireDatabase=Host=localhost;Database=jobs;Username=postgres;Password=<secret>
```

This separation lets application persistence and background-job persistence evolve independently.

## Runtime behavior

At normal runtime, missing application or Hangfire connection strings fail closed during startup configuration.

Build-time OpenAPI generation may use a non-network placeholder connection string only to construct the EF model. It does not seed or migrate the database.

Readiness checks use `AppDbContext.Database.CanConnectAsync`, so they follow whichever EF provider is selected.
