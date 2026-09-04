# Docker

The generated project includes a production-style multi-stage `Dockerfile` and a Docker Compose development stack.

## What is included

- .NET 10 SDK build stage and ASP.NET Core runtime stage
- non-root `app` runtime user
- read-only API container filesystem with only explicit writable mounts
- persistent Serilog logs
- persistent ASP.NET Core Data Protection keys
- separate EF Core migration image/job
- PostgreSQL 18, SQL Server 2022 and MySQL 8.4 database profiles
- SQLite through a persistent application data volume
- localhost-only host port bindings for the API and development databases
- Docker health checks for database containers
- Hangfire inheritance for PostgreSQL/SQL Server; disabled by default for MySQL/SQLite

The Compose stack is intended for local development and CI. Production deployments should use the same final image with platform-managed secrets, an external production database, a shared/secured Data Protection key store for multiple replicas, and your orchestrator's HTTP probes against `/health/live` and `/health/ready`.

## First-time setup

The project template intentionally does not ship provider-specific EF migrations. First copy the environment example for the database you want, set its blank secrets, and generate the migration using the included helper.

Bash/zsh:

```bash
bash docker/create-migration.sh InitialCreate
```

PowerShell:

```powershell
pwsh -File docker/create-migration.ps1 -MigrationName InitialCreate
```

The helpers use `docker compose config --environment` to resolve the selected Docker environment and pass the provider/connection string to `dotnet ef`. They require the .NET 10 SDK, Docker Compose and `dotnet-ef` on the host.

The Docker `migrate` service then applies committed migrations automatically before the API starts. It retries while the database is starting and fails clearly when no migrations exist.

## PostgreSQL (recommended Docker default)

```bash
cp .env.example .env
```

Set `POSTGRES_PASSWORD` and `JWT_SECRET` in `.env`, then:

```bash
bash docker/create-migration.sh InitialCreate
docker compose up --build
```

The default `.env.example` enables the `postgres` profile. PostgreSQL 18 persists its data under `/var/lib/postgresql`, matching the PostgreSQL 18+ official image layout.

## SQL Server

```bash
cp docker/env/sqlserver.env.example .env
```

Set `MSSQL_SA_PASSWORD` and `JWT_SECRET`, then:

```bash
bash docker/create-migration.sh InitialCreate
docker compose up --build
```

The SQL Server password must satisfy SQL Server's password policy.

Microsoft supports SQL Server Linux containers on x86-64 Linux hosts. ARM64 Docker Desktop users, including Apple Silicon Macs, should prefer PostgreSQL, MySQL or SQLite for local containers unless they intentionally accept unsupported emulation.

## MySQL

```bash
cp docker/env/mysql.env.example .env
```

Set `MYSQL_PASSWORD`, `MYSQL_ROOT_PASSWORD` and `JWT_SECRET`, then:

```bash
bash docker/create-migration.sh InitialCreate
docker compose up --build
```

Hangfire is disabled by default for the MySQL profile. To enable jobs, configure Hangfire with a separate SQL Server or PostgreSQL connection.

## SQLite

```bash
cp docker/env/sqlite.env.example .env
```

Set `JWT_SECRET`, then:

```bash
bash docker/create-migration.sh InitialCreate
docker compose up --build
```

SQLite needs no database service or Compose profile. It persists at `/app/data/app.db` through the `sqlite-data` named volume. Hangfire is disabled by default.

## Common commands

```bash
# Start or rebuild
docker compose up --build

# Detached mode
docker compose up -d --build

# View API logs
docker compose logs -f api

# Re-run migrations
docker compose run --rm migrate

# Stop containers but keep data
docker compose down

# Stop containers and delete all local named volumes
docker compose down -v

# Validate the resolved Compose model
docker compose config
```

The API is bound to `127.0.0.1:8080` by default. Change `APP_PORT` in `.env` when necessary.

## Secrets

`.env` and `.env.*` are ignored by the generated project. The example files deliberately leave secrets blank. Do not commit real JWT secrets, database passwords, mail credentials, bootstrap-admin credentials or Hangfire dashboard credentials.

For production, do not copy a local `.env` file into the image. `.dockerignore` excludes environment files from the Docker build context.

## Data Protection

The API container sets `HOME=/home/app` and persists `/home/app/.aspnet/DataProtection-Keys` as a named volume. This keeps password-reset and other Data Protection payloads valid across local container restarts.

For horizontally scaled production deployments, replace the local named volume with a shared persistent provider and protect keys at rest according to your hosting environment.

## Database selection

Docker Compose profiles select only local database containers:

- `postgres`
- `sqlserver`
- `mysql`

SQLite has no database-container profile. Application database selection remains the existing runtime configuration contract:

```text
DatabaseSettings__Provider
DatabaseSettings__ConnectionStringName
ConnectionStrings__appDatabase
```

`COMPOSE_PROFILES` is supplied by the PostgreSQL, SQL Server and MySQL example environment files.
