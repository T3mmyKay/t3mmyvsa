# Releasing T3mmyVSA Packages

T3mmyVSA publishes two packages from one version source:

- `T3mmyvsa.CLI`
- `T3mmy.VSA.Templates`

The authoritative version is `VersionPrefix` in the repository root `Directory.Build.props`.

## Release checklist

1. Merge the intended starter-kit changes and ensure the full solution is green.
2. Review NuGet audit output. High and critical advisories must be resolved before release.
3. Update `VersionPrefix` using semantic versioning.
4. Review `T3mmyvsa.Web/Directory.Packages.props` and keep Microsoft .NET/EF packages on one supported patch band.
5. Run the **Publish NuGet Packages** workflow manually with `publish=false` before a new public package line. This is the package/deployment dry-run and does not push packages.
6. Create and publish a GitHub release tagged exactly `v<VersionPrefix>` (for example `v2.2.0`).
7. The release workflow repeats the full package gate from the tagged source.
8. Only after all verification succeeds are packages pushed to GitHub Packages and NuGet.org.

## Required repository secret

NuGet.org publishing requires:

```text
NUGET_API_KEY
```

GitHub Packages uses the workflow `GITHUB_TOKEN` with `packages: write` permission.

## Package and deployment smoke test

The release workflow verifies the artifacts that will actually be published rather than only the source projects. It:

1. restores and builds the complete repository solution;
2. runs repository unit and architecture tests;
3. packs both NuGet packages;
4. installs `T3mmyvsa.CLI` from the freshly created `.nupkg`;
5. installs `T3mmy.VSA.Templates` from the freshly created `.nupkg`;
6. creates a new project from the packaged template;
7. runs the packaged CLI to generate `SmokeProduct` plus CRUD slices;
8. builds the generated application in Release configuration;
9. runs the generated unit, architecture and PostgreSQL/Testcontainers integration suites;
10. generates a provider-specific EF migration into `Migrations/Data/Migrations` using the packaged template;
11. validates every Docker Compose environment example;
12. builds the generated API, Worker and Migrations Docker targets;
13. starts a disposable PostgreSQL database and runs the generated Migrations runtime image against it.

A failure at any of these stages blocks all registry push steps. This gate is intended to detect package-content mistakes, CLI/template version drift, generated-code regressions, migration-assembly errors and broken deployment images before publication.

## Versioning policy

Use semantic versioning:

- **major**: breaking API/auth/template/CLI contract changes;
- **minor**: backward-compatible starter capabilities;
- **patch**: fixes, security patches and dependency updates that preserve public contracts.

The PR #1-#6 starter hardening series established the `2.0.0` line because it changed authentication/session behavior, user contracts, validation/error conventions, deployment defaults and generated CLI output.

The `2.2.0` line coordinates the testing foundation with dedicated migrations/background-worker topology and its deployment compatibility paths. These are additive starter capabilities rather than another breaking public-contract reset.

## Hosting compatibility

The packaged template supports both process models:

- Docker/VPS/orchestrated hosting: dedicated API, Worker and run-to-completion Migrations processes;
- SmarterASP.NET/shared IIS hosting: migrations run from the deployment workflow and Hangfire processing is intentionally enabled in the web process because ordinary shared hosting does not provide the same independent worker-process lifecycle.

See `T3mmyvsa.Web/docs/PROCESS_TOPOLOGY.md` in the repository or `docs/PROCESS_TOPOLOGY.md` in a generated project.

## Artifact policy

Do not commit package outputs. `.nupkg`, `.snupkg` and `artifacts/` are ignored. Published packages must be reproducible from the tagged source through the release workflow.
