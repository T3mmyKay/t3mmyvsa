# Releasing T3mmyVSA Packages

T3mmyVSA publishes two packages from one version source:

- `T3mmyvsa.CLI`
- `T3mmy.VSA.Templates`

The authoritative version is `VersionPrefix` in the repository root `Directory.Build.props`.

## Release checklist

1. Merge the stacked starter-kit changes and ensure the full solution builds successfully.
2. Review NuGet audit output. High and critical advisories must be resolved before release.
3. Update `VersionPrefix` using semantic versioning.
4. Review `T3mmyvsa.Web/Directory.Packages.props` and keep Microsoft .NET/EF packages on one supported patch band.
5. Optionally run the **Publish NuGet Packages** workflow manually with `publish=false`. This is a package dry-run: it restores, builds, packs and runs package smoke tests without pushing packages.
6. Create and publish a GitHub release tagged exactly `v<VersionPrefix>` (for example `v2.0.0`).
7. The release workflow validates the tag, rebuilds the solution, packs the CLI/template, installs the packaged artifacts, scaffolds a smoke entity/feature and builds the generated app before any push step.
8. Only after all verification succeeds are packages pushed to GitHub Packages and NuGet.org.

## Required repository secret

NuGet.org publishing requires:

```text
NUGET_API_KEY
```

GitHub Packages uses the workflow `GITHUB_TOKEN` with `packages: write` permission.

## Package smoke test

The release workflow verifies the artifacts that will actually be published rather than only the source projects. It:

1. installs `T3mmyvsa.CLI` from the freshly created `.nupkg`;
2. installs `T3mmy.VSA.Templates` from the freshly created `.nupkg`;
3. creates a new project from the packaged template;
4. runs the packaged CLI to generate `SmokeProduct` plus CRUD slices;
5. builds the generated application in Release configuration.

This gate is intended to detect template-content errors, CLI/template version drift and scaffolding output that does not compile against the packaged starter.

## Versioning policy

Use semantic versioning:

- **major**: breaking API/auth/template/CLI contract changes;
- **minor**: backward-compatible starter capabilities;
- **patch**: fixes, security patches and dependency updates that preserve public contracts.

The PR #1-#6 starter hardening series is intentionally represented as the `2.0.0` line because it changes authentication/session behavior, user contracts, validation/error conventions, deployment defaults and generated CLI output.

## Artifact policy

Do not commit package outputs. `.nupkg`, `.snupkg` and `artifacts/` are ignored. Published packages must be reproducible from the tagged source through the release workflow.
