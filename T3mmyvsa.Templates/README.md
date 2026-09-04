# T3mmyVSA Templates

Project templates for production-oriented ASP.NET Core 10 APIs using Vertical Slice Architecture.

## Installation

```bash
dotnet new install T3mmy.VSA.Templates
```

## Usage

```bash
dotnet new t3mmyvsa -n MyProject
```

## Included foundation

- Vertical Slice Architecture
- Carter minimal API endpoints
- Cortex Mediator
- ASP.NET Core Identity
- granular permission authorization
- EF Core 10 with SQL Server, PostgreSQL, MySQL and SQLite
- provider-specific design-time migration support
- FluentValidation
- centralized ProblemDetails
- Serilog
- API versioning
- JWT-backed server sessions
- Hangfire with SQL Server or PostgreSQL storage
- health/readiness checks
- hardened CORS, proxy, rate-limit and production configuration defaults
- production-style Dockerfile and multi-provider Docker Compose development stack

The generated project intentionally contains no pre-generated EF migrations. Select the database provider and connection string first, then run:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

See `docs/DATABASE_PROVIDERS.md` in the generated project for provider aliases, migration rules and independent Hangfire storage configuration.

## Docker

The generated project includes:

- a multi-stage .NET 10 Dockerfile;
- a non-root/read-only API runtime container;
- a dedicated EF migration target/job;
- persistent Data Protection keys and logs;
- PostgreSQL, SQL Server and MySQL Compose profiles;
- SQLite volume support.

PostgreSQL is the default Docker development profile. Start with:

```bash
cp .env.example .env
# fill POSTGRES_PASSWORD and JWT_SECRET
docker compose up --build
```

See `docs/DOCKER.md` in the generated project for all provider profiles and production guidance.

## CLI Tool

The template does **not** install or update a global tool automatically. Install the companion CLI explicitly:

```bash
dotnet tool install -g T3mmyvsa.CLI
```

Then scaffold from the generated web project:

```bash
dotnet t3mmyvsa make:entity Product
dotnet t3mmyvsa make:feature Product
```

See the CLI package README for `--base`, `--force`, navigation-property behavior and generated CRUD conventions.

## License

MIT
