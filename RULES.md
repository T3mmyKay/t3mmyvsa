# Coding Rules & Architectural Guidelines

## 1. Vertical Slice Architecture

- **Structure**: Organize code by **Features** (Use Cases) rather than technical layers.
- **Path**: `Features/[Domain]/[UseCase]` (e.g., `Features/Users/CreateUser`).
- **Components**: Each feature slice should be self-contained and typically includes:
    - **Endpoint**: `ICarterModule` implementation for route definition.
    - **Handler**: `ICommandHandler` or `IQueryHandler` for business logic.
    - **Command/Query**: Record types defining the input.
    - **Validator**: `AbstractValidator<T>` using FluentValidation for input validation.
    - **Response**: DTOs for output (e.g., `UserResponse`).
- **Exceptions**: Cross-cutting concerns and shared infrastructure (Identity, generic Services) reside in `T3mmyvsa.Web` root folders.

## 2. File Organization

- **One Class Per File**: Every class, interface, enum, or struct must reside in its own separate file.
- **File Naming**: Check that file names exactly match the type name they contain.
- **Constructors**: Use **Primary Constructors** for all class definitions where dependency injection is used.

## 3. CLI Tools & Scaffolding

Use the **T3mmyvsa CLI** to maintain consistency and speed up development.

- **Make Entity**:
  ```bash
  dotnet t3mmyvsa make:entity <EntityName>
  ```
  - Creates Entity in `Entities/`.
  - Creates Configuration in `Data/Configurations/`.
  - Updates `AppDbContext` to include the `DbSet`.

- **Make Feature**:
  ```bash
  dotnet t3mmyvsa make:feature <EntityName>
  ```
  - Scaffolds a complete CRUD set in `Features/<EntityName>s/`.
  - Generates: `Create`, `Update`, `Delete`, `Get`, `GetList`, `BulkDelete`.
  - Automatically adds permissions to `AppPermission.cs`.

## 4. Coding Style & Standards

- **Object Initialization**: Use **simplified object initialization** (e.g., `new T { Prop = val }`).
- **Pagination**: Use `PaginatedResponse<T>` for all list endpoints and enforce bounded page sizes.
- **Query Parameters**: Use `[AsParameters]` in endpoints to bind query objects. Keep HTTP binding metadata on query properties rather than positional constructor parameters.
- **Result Pattern**: Mutations (Create/Update/Delete) should return `ResultResponse`.

## 5. Interfaces

- **Location**: Interfaces for shared services must be in `Interfaces` folder.
- **Naming**: Interface names must be prefixed with `I` (e.g., `IEmailService`).

## 6. Authorization & Identity

- **RBAC**: Use ASP.NET Core Identity Roles.
- **Permissions**: Defined in `Authorization/Enums/AppPermission.cs`.
- **Endpoint Security**: Use `.HasPermissions(AppPermission.X)` extension on Carter endpoints.
- **Resource Authorization**: Handlers must still enforce ownership/resource rules when endpoint permissions alone are insufficient.
- **Handler-Level Denial**: Throw `ForbiddenException` when an authenticated caller fails a handler-level authorization check.

## 7. Validation & Error Handling

- **Feature Validation**: Commands and queries use FluentValidation `AbstractValidator<T>`. Do not put DataAnnotations validation attributes on feature command/query contracts.
- **Single Validation Path**: Validation is performed by the shared endpoint validation filter; do not duplicate the same validation in endpoint lambdas and handlers unless enforcing a defensive domain invariant.
- **Thin Endpoints**: Do not add repetitive `try/catch` blocks to translate handler exceptions into HTTP results.
- **ProblemDetails**: `GlobalExceptionHandler` is the authoritative exception-to-HTTP mapping and all error responses should use ProblemDetails.
- **Missing Resources**: Throw `KeyNotFoundException` when an expected resource does not exist.
- **State Conflicts**: Throw `ConflictException` for duplicate resources or operations that violate the current resource state/invariant.
- **Authentication Failures**: Throw `UnauthorizedAccessException` for authentication failures. Do not use it for authenticated authorization denial.
- **Unexpected Failures**: Never expose internal exception details for 5xx responses.

## 8. Auditing

- **Actor Identity**: Audit actor IDs must use the Identity `NameIdentifier`/user ID, never username or email.
- **Sensitive Values**: Never audit password hashes, tokens, secrets, credentials, security stamps, concurrency stamps, API keys, or private keys.
- **Explicit Exclusion**: Mark custom sensitive entity properties with `[AuditIgnore]`.
- **Identity Changes**: Audit safe user/profile state, role assignment, user permission claims, role changes, and role permission claims.
- **Activity Lists**: Audit/activity list endpoints must be paginated and ordered deterministically.

## 9. Logging

- **Serilog**: Mandatory configuration.
- **Sinks**: Console and File sinks required.
- **Enrichment**: Use `FromLogContext`.

## 10. Dependency Injection

- **Scrutor**: Used for automatic service registration.
- **Lifetimes**:
  - `[ScopedService]`, `[SingletonService]`, `[TransientService]` attributes.
  - Default: Classes ending in "Service" are **Transient** if no attribute is present.

## 11. Configuration & Options

- **Options Pattern**: Use `services.Configure<T>()`.
- **Settings Classes**: Located in `Configuration/`, named `*Settings`.
- **Injection**: Inject via `IOptions<T>`.

## 12. Entity & Data Configuration

- **Base Entities**: Inherit from `BaseEntity` or `AuditableEntity`.
- **IDs**: Use `Guid.CreateVersion7()` for time-ordered UUIDs.
- **Configuration**: `IEntityTypeConfiguration<T>` in `Data/Configurations/`.
- **Auditing**: Handled automatically via `AuditInterceptor`.
