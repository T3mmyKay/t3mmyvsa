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
- EF Core 10
- FluentValidation
- centralized ProblemDetails
- Serilog
- API versioning
- JWT-backed server sessions
- Hangfire SQL Server background jobs
- health/readiness checks
- hardened CORS, proxy, rate-limit, and production configuration defaults

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

See the CLI package README for `--base`, `--force`, navigation-property behavior, and generated CRUD conventions.

## License

MIT
