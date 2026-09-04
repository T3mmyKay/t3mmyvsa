# T3mmyVSA Templates

Production-oriented ASP.NET Core 10 templates using Vertical Slice Architecture.

## Installation

```bash
dotnet new install T3mmy.VSA.Templates
dotnet new t3mmyvsa -n MyProject
```

## Included foundation

- Vertical Slice Architecture with Carter + Cortex Mediator
- ASP.NET Core Identity, JWT sessions and granular permissions
- EF Core 10 with SQL Server, PostgreSQL, MySQL and SQLite
- dedicated `Migrations` executable/project that owns EF migrations
- dedicated Hangfire `Worker` process; API is enqueue-only by default
- FluentValidation + centralized ProblemDetails
- Serilog, API versioning, health/readiness checks
- xUnit v3 + Microsoft Testing Platform unit, architecture and Testcontainers-backed integration tests
- production-style Dockerfile and multi-provider Docker Compose topology
- hardened CORS, proxy, rate-limit and configuration defaults

## Migrations

Generated projects intentionally contain no provider-specific migration history. Choose the provider first, then generate the initial migration into the dedicated migrations project:

```bash
cp .env.example .env
bash docker/create-migration.sh InitialCreate
```

Equivalent direct EF command:

```bash
dotnet ef migrations add InitialCreate \
  --project Migrations/T3mmyvsa.Migrations.csproj \
  --startup-project Migrations/T3mmyvsa.Migrations.csproj \
  --output-dir Data/Migrations
```

The deployed migration container runs the compiled `T3mmyvsa.Migrations` executable; it does not carry the .NET SDK or `dotnet-ef`.

## Worker topology

The API registers Hangfire storage/client support but defaults `Hangfire:ServerEnabled=false`. The dedicated Worker process forces server mode and processes the configured queues. This isolates background-job CPU, memory and restart behavior from HTTP request handling.

For a simple deployment you can intentionally re-enable in-process processing with `Hangfire__ServerEnabled=true` and omit the Worker, but the generated Compose topology uses the dedicated worker.

## Testing

```bash
dotnet test T3mmyvsa.slnx -c Release
```

Integration tests use a disposable PostgreSQL Testcontainer and require Docker. See `docs/TESTING.md`.

## Docker

```bash
cp .env.example .env
# set POSTGRES_PASSWORD and JWT_SECRET
bash docker/create-migration.sh InitialCreate
docker compose up --build
```

Compose runs the migration executable first, then starts the API and dedicated worker. See `docs/DOCKER.md` and `docs/PROCESS_TOPOLOGY.md`.

## CLI Tool

```bash
dotnet tool install -g T3mmyvsa.CLI
dotnet t3mmyvsa make:entity Product
dotnet t3mmyvsa make:feature Product
```

## License

MIT
