# Docker

The generated project includes a multi-stage .NET 10 Dockerfile and Compose topology for API, Worker and run-to-completion migrations.

## Runtime targets

- `final` — non-root/read-only ASP.NET API image
- `worker` — non-root/read-only Hangfire Generic Host
- `migrations` — compiled migration executable in the ASP.NET runtime image (no SDK or `dotnet-ef`)

Compose also provides PostgreSQL 18, SQL Server 2022 and MySQL 8.4 profiles; SQLite uses a persistent volume.

## First-time setup

The template intentionally ships without provider-specific migrations. Configure a provider and generate one on the development host:

```bash
cp .env.example .env
# set POSTGRES_PASSWORD and JWT_SECRET
bash docker/create-migration.sh InitialCreate
docker compose up --build
```

PowerShell:

```powershell
pwsh -File docker/create-migration.ps1 -MigrationName InitialCreate
```

Generation needs .NET 10, Docker Compose and `dotnet-ef`. Deployment does not: the `migrate` container runs the compiled migrations executable, retries while the database starts, and must succeed before API/Worker services start.

## Services

`api` handles HTTP and enqueues Hangfire jobs. It defaults `Hangfire__ServerEnabled=false`.

`worker` processes Hangfire queues independently. It has no public port and can be scaled independently.

`migrate` is run-to-completion and applies EF migrations before the long-running services.

## Providers

PostgreSQL default:

```bash
cp .env.example .env
```

SQL Server:

```bash
cp docker/env/sqlserver.env.example .env
```

MySQL:

```bash
cp docker/env/mysql.env.example .env
```

SQLite:

```bash
cp docker/env/sqlite.env.example .env
```

For MySQL/SQLite, Hangfire is disabled by default because this starter supports SQL Server/PostgreSQL Hangfire storage only. Configure a separate supported job store to enable the Worker.

SQL Server Linux containers are x86-64 oriented; Apple Silicon/ARM developers should prefer PostgreSQL, MySQL or SQLite locally unless deliberately using emulation.

## Common commands

```bash
docker compose up --build
docker compose up -d --build
docker compose logs -f api worker
docker compose run --rm migrate
docker compose down
docker compose down -v
docker compose config
```

The API binds to `127.0.0.1:8080` by default. Database ports are localhost-only.

## Hardening

API, Worker and migration containers run non-root. Long-running app containers are read-only with explicit writable volumes/tmpfs, `no-new-privileges`, and all Linux capabilities dropped. Secrets come from environment/platform secret stores and `.env*` files are excluded from image build context.

API Data Protection keys are persisted locally. Horizontally scaled production deployments should use a shared protected key store.

See `docs/PROCESS_TOPOLOGY.md` for deployment/scaling guidance.
