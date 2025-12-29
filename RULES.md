# Coding Rules & Architectural Guidelines

## 1. Vertical Slice Architecture

- **Structure**: Organize code by **Features** (Use Cases) rather than technical layers.
- **Path**: `Features/[Domain]/[UseCase]` (e.g., `Features/Users/CreateUser`).
- **Components**: Each feature slice should be self-contained and typically includes:
    - **Endpoint**: `ICarterModule` implementation for route definition.
    - **Handler**: `ICommandHandler` or `IQueryHandler` for business logic.
    - **Command/Query**: Record types defining the input.
    - **Validator**: `AbstractValidator<T>` for input validation.
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
- **Pagination**: Use `PaginatedResponse<T>` for all list endpoints.
- **Query Parameters**: Use `[AsParameters]` in endpoints to bind query objects (e.g., `GetUsersRequest`).
- **Result Pattern**: Mutations (Create/Update/Delete) should return `ResultResponse`.

## 5. Interfaces

- **Location**: Interfaces for shared services must be in `Interfaces` folder.
- **Naming**: Interface names must be prefixed with `I` (e.g., `IEmailService`).

## 6. Authorization & Identity

- **RBAC**: Use ASP.NET Core Identity Roles.
- **Permissions**: Defined in `Authorization/Enums/AppPermission.cs`.
- **Endpoint Security**: Use `.HasPermissions(AppPermission.X)` extension on Carter endpoints.

## 7. Logging

- **Serilog**: Mandatory configuration.
- **Sinks**: Console and File sinks required.
- **Enrichment**: Use `FromLogContext`.

## 8. Dependency Injection

- **Scrutor**: Used for automatic service registration.
- **Lifetimes**:
  - `[ScopedService]`, `[SingletonService]`, `[TransientService]` attributes.
  - Default: Classes ending in "Service" are **Transient** if no attribute is present.

## 9. Configuration & Options

- **Options Pattern**: Use `services.Configure<T>()`.
- **Settings Classes**: Located in `Configuration/`, named `*Settings`.
- **Injection**: Inject via `IOptions<T>`.

## 10. Entity & Data Configuration

- **Base Entities**: Inherit from `BaseEntity` or `AuditableEntity`.
- **IDs**: Use `Guid.CreateVersion7()` for time-ordered UUIDs.
- **Configuration**: `IEntityTypeConfiguration<T>` in `Data/Configurations/`.
- **Auditing**: Handled automatically via `AuditInterceptor`.
