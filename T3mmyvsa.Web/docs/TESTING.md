# Testing

T3mmyVSA ships with a three-layer automated test foundation using xUnit v3 on Microsoft Testing Platform (MTP).

## Test projects

- `Tests/T3mmyvsa.UnitTests` — fast tests for validators, provider resolution, handlers and pure business logic.
- `Tests/T3mmyvsa.ArchitectureTests` — structural guardrails for Vertical Slice Architecture conventions.
- `Tests/T3mmyvsa.IntegrationTests` — real ASP.NET Core HTTP pipeline tests backed by an ephemeral PostgreSQL Testcontainer.

The generated `global.json` selects `Microsoft.Testing.Platform`, which is the native `dotnet test` path on .NET 10.

## Run all tests

```bash
dotnet test T3mmyvsa.slnx -c Release
```

Run a single layer when iterating:

```bash
dotnet test Tests/T3mmyvsa.UnitTests/T3mmyvsa.UnitTests.csproj
dotnet test Tests/T3mmyvsa.ArchitectureTests/T3mmyvsa.ArchitectureTests.csproj
dotnet test Tests/T3mmyvsa.IntegrationTests/T3mmyvsa.IntegrationTests.csproj
```

Integration tests require Docker because Testcontainers starts a disposable PostgreSQL instance. No developer database is reused or modified.

## Code coverage

Microsoft Testing Platform code coverage is included. Example:

```bash
dotnet test Tests/T3mmyvsa.UnitTests/T3mmyvsa.UnitTests.csproj \
  --coverage \
  --coverage-output TestResults/unit.cobertura.xml \
  --coverage-output-format cobertura
```

## Conventions

- Unit tests must not need network services or Docker.
- Architecture tests must remain deterministic and external-service free.
- Integration tests exercise real infrastructure where provider behavior matters; prefer Testcontainers over EF InMemory for relational behavior.
- Add regression tests with every bug fix.
- Keep expensive integration tests after build/unit/architecture gates in CI so fast failures do not consume container time.
