# Coding Rules & Architectural Guidelines

## 1. Vertical Slice Architecture

- **Structure**: Organize code by **Features** (Use Cases) rather than technical layers.
- **Path**: `Features/[Domain]/[UseCase]` (e.g., `Features/Users/CreateUser`).
- **Components**: Each feature slice should be self-contained and typically includes:
    - **Endpoint**: `ICarterModule` implementation for route definition.
    - **Handler**: `ICommandHandler` or `IQueryHandler` for business logic.
    - **Command/Query**: Types defining application input.
    - **Validator**: `AbstractValidator<T>` using FluentValidation for endpoint input validation.
    - **Response**: DTOs for output (e.g., `UserResponse`).
- **Exceptions**: Cross-cutting concerns and shared infrastructure (Identity, generic Services) reside in `T3mmyvsa.Web` root folders.

## 2. File Organization

- **One Class Per File**: Every class, interface, enum, or struct must reside in its own separate file.
- **File Naming**: File names must match the primary type they contain.
- **Constructors**: Use primary constructors for class definitions where dependency injection is used.

## 3. CLI Tools & Scaffolding

Use the **T3mmyvsa CLI** to create the initial mechanical slice, then add business/resource rules deliberately.

- `dotnet t3mmyvsa make:entity <EntityName>` creates an `AuditableEntity` by default, its EF configuration and `DbSet`.
- `dotnet t3mmyvsa make:entity <EntityName> --base` is reserved for entities that intentionally do not need audit timestamps.
- `dotnet t3mmyvsa make:feature <EntityName>` scaffolds Create/Update/Delete/Get/GetList/BulkDelete slices and `View/Create/Update/Delete` permissions.
- Generated entity identifiers are `Guid` because `BaseEntity.Id` is a version-7 GUID.
- Generated list contracts derive from `PaginationRequest`, enforce `per_page <= 100`, propagate cancellation and use deterministic secondary ordering.
- Optional navigation properties/collections are not exposed automatically in transport contracts. Required navigation properties stop generic scaffolding so the relationship can be modeled explicitly.
- Existing generated feature files are not overwritten by default. `--force` is explicit destructive replacement and must not be used casually on hand-edited slices.
- Entity names must be valid PascalCase C# identifiers. Adjust irregular English plurals manually after generation when required.
- The project template must not silently install/update a global CLI version.

## 4. Coding Style & Standards

- **Object Initialization**: Use simplified object initialization.
- **Identifiers**: Domain `BaseEntity` identifiers are `Guid`; do not regress generated or handwritten slices to string IDs.
- **Pagination**: Use `PaginatedResponse<T>` for list endpoints and enforce bounded page sizes.
- **Query Parameters**: Use `[AsParameters]` in endpoints to bind query objects. Keep HTTP binding metadata on query properties rather than positional constructor parameters.
- **List Reads**: Use `AsNoTracking()`, deterministic ordering, cancellation tokens and the actual request path when building pagination links.
- **Result Pattern**: Generic CRUD mutations use `ResultResponse`; domain-specific mutations may return a more expressive feature response when needed.
- **API Versions**: Shared endpoints currently intended for both public API versions should declare both `.HasApiVersion(1)` and `.HasApiVersion(2)`.

## 5. Interfaces

- **Location**: Interfaces for shared services belong in `Interfaces`.
- **Naming**: Interface names are prefixed with `I`.

## 6. Authorization & Identity

- **RBAC**: Use ASP.NET Core Identity Roles.
- **Permissions**: Define granular permissions in `Authorization/Enums/AppPermission.cs`.
- **CRUD Permissions**: Generic CRUD uses separate `View`, `Create`, `Update`, and `Delete` permissions. Never reuse Create as a surrogate for Delete.
- **Endpoint Security**: Use `.HasPermissions(AppPermission.X)` on protected Carter endpoints.
- **Resource Authorization**: Handlers still enforce ownership/resource rules when endpoint permissions are insufficient.
- **Handler-Level Denial**: Throw `ForbiddenException` when an authenticated caller fails a handler-level authorization check.

## 7. Validation & Error Handling

- **Feature Validation**: Endpoint-bound commands/queries/requests use FluentValidation `AbstractValidator<T>`; do not add DataAnnotations validation attributes to feature contracts.
- **Single Validation Path**: Shared endpoint validation handles transport validation. Handlers retain defensive domain invariants for inputs that can also arrive from non-HTTP callers.
- **Thin Endpoints**: Do not add repetitive `try/catch` HTTP translation.
- **ProblemDetails**: `GlobalExceptionHandler` is the authoritative exception-to-HTTP mapping.
- **Invalid Request Shape/Route Contract**: `ArgumentException` for defensive route/body mismatch or invalid application input that escaped endpoint validation.
- **Missing Resources**: `KeyNotFoundException`.
- **State Conflicts**: `ConflictException`.
- **Authentication Failures**: `UnauthorizedAccessException` only for authentication failures.
- **Unexpected Failures**: Never expose internal exception details in 5xx responses.

## 8. Auditing

- **Actor Identity**: Use Identity `NameIdentifier`/user ID, never username or email.
- **Sensitive Values**: Never audit password hashes, tokens, secrets, credentials, security/concurrency stamps, API keys or private keys.
- **Explicit Exclusion**: Mark custom sensitive entity properties with `[AuditIgnore]`.
- **Identity Changes**: Audit safe user/profile state, role assignment, user permission claims, role changes and role permission claims.
- **Activity Lists**: Paginate and order deterministically.

## 9. Logging

- **Serilog**: Mandatory structured logging.
- **Sinks**: Console and file sinks are configured by default.
- **Enrichment**: Use `FromLogContext`.
- **Secrets**: Never log passwords, refresh tokens, JWT signing keys, connection strings or dashboard credentials.

## 10. Dependency Injection

- **Scrutor**: Used for automatic service registration.
- **Lifetimes**: `[ScopedService]`, `[SingletonService]`, `[TransientService]`; otherwise service naming conventions apply.

## 11. Configuration & Options

- **Settings Classes**: Located in `Configuration/`, named `*Settings`.
- **Injection**: Inject through `IOptions<T>` where runtime settings are required.
- **Validation**: Security-sensitive settings use FluentValidation and `ValidateOnStart`.
- **Secrets**: Never commit connection strings containing credentials, JWT secrets, SMTP passwords, bootstrap passwords, API keys or dashboard credentials. Use environment variables, user-secrets or the deployment secret store.
- **Bootstrap Admin**: Disabled by default. Enable only for initial provisioning and disable after the administrator account exists.

## 12. Entity & Data Configuration

- **Base Entities**: Prefer `AuditableEntity` for normal business records; use `BaseEntity` when audit timestamps are intentionally unnecessary.
- **IDs**: Use `Guid.CreateVersion7()` for time-ordered UUIDs.
- **Configuration**: `IEntityTypeConfiguration<T>` in `Data/Configurations/`.
- **Auditing**: Handled automatically via `AuditInterceptor`.
- **Navigation Input**: Do not bind EF navigation graphs directly from generic create/update API contracts. Model scalar foreign keys/value contracts explicitly.

## 13. HTTP & Deployment Security

- **CORS**: Explicit origins only; wildcard origins are not accepted by the starter policy.
- **Rate Limiting**: Public auth/recovery/token endpoints must retain their named rate-limit policies.
- **Forwarded Headers**: Accept `X-Forwarded-*` only from configured trusted proxy addresses.
- **Transport**: HSTS is enabled outside Development; reverse proxies must forward the original HTTPS scheme correctly.
- **Hosts**: Production deployments must set `AllowedHosts` to the real hostnames.
- **API Docs**: OpenAPI/Scalar are development-only unless explicitly enabled for a controlled production environment.
- **Health**: Keep separate liveness and readiness probes; readiness may touch dependencies, liveness must not.
- **Deployment TLS**: Never disable deployment certificate validation or use `-allowUntrusted` in production deployment tooling.

## 14. Background Jobs

- **Engine**: Hangfire with first-party SQL Server storage is the starter background-job foundation.
- **Configuration**: Use `HangfireSettings`; do not add scheduler credentials to source control.
- **Dashboard**: Disabled by default; when enabled it requires external credentials, HTTPS by default and is read-only by default. Prefer an IP allowlist or private network/VPN in production.
- **Job Arguments**: Never serialize secrets, access tokens or unnecessary personal data into background-job arguments.
- **Idempotency**: Jobs must tolerate retry/re-execution safely.
- **Retries**: Keep retries bounded; override only when the use case justifies a different policy.
- **Queues**: Use lowercase queue names and configure worker queues explicitly.
- **Isolation**: `Hangfire:ServerEnabled=false` may be used on web nodes when a dedicated worker deployment processes jobs.
