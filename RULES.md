# Coding Rules & Architectural Guidelines

## 1. Vertical Slice Architecture

- **Structure**: Organize code by **Features** (Use Cases) rather than technical layers.
- **Exceptions**: Cross-cutting concerns and shared infrastructure (like Identity configuration, generic Services) should reside in dedicated top-level folders defined below.

## 2. File Organization

- **One Class Per File**: Every class, interface, enum, or struct must reside in its own separate file.
- **File Naming**: Check that file names exactly match the type name they contain.
- **Constructors**: Use **Primary Constructors** for all class definitions where dependency injection is used.

## 3. Coding Style

- **Object Initialization**: Use **simplified object initialization** syntax (e.g., `new T { Prop = val }`) instead of setting properties after construction.

## 4. Interfaces

- **Location**: Interfaces, especially for shared services, should represent a contract and must be placed in a distinct `Interfaces` folder or namespace (e.g., `Services/Interfaces`).
- **Naming**: Interface names must be prefixed with `I` (e.g., `IEmailService`).

## 5. Authorization & Identity

- **Role-Based Access Control (RBAC)**: Use ASP.NET Core Identity Roles.
- **Claims-Based**: Assign permissions as Claims to Roles.
- **Configuration**: Ensure `AddRoles<IdentityRole>()` is called in Identity setup.

## 6. Logging

- **Serilog**: configuration is mandatory.
- **Sinks**: Must support at least Console and File sinks.
- **Configuration**: Setup Serilog in `appsettings.json` and initialize it in `Program.cs`.

## 7. Dependency Injection

- **Scrutor**: Use Scrutor for service registration.
- **Auto-Scanning**: Avoid manual Service Registration.
- **Lifetimes**:
  - **Attributes**: Use `[ScopedService]`, `[SingletonService]`, or `[TransientService]` to explicitly define lifetime.
  - **Default**: Services ending in "Service" without attributes defaults to **Transient**.

## 8. Configuration & Options Pattern

- **Options Pattern**: Use `services.Configure<T>()` for all configuration binding.
- **Settings Classes**: Create dedicated settings classes in `Configuration/` folder.
- **Naming**: Settings class names must end with `Settings` (e.g., `JwtSettings`, `MailSettings`).
- **Injection**: Inject settings via `IOptions<T>`, `IOptionsSnapshot<T>`, or `IOptionsMonitor<T>`.
- **Avoid**: Do not read directly from `IConfiguration` using indexers (e.g., `config["Key"]`).

## 9. Entity & Data Configuration

- **Base Entities**: All domain entities should inherit from `BaseEntity` or `AuditableEntity`.
- **Guid.CreateVersion7**: Use `Guid.CreateVersion7()` for primary key generation (time-ordered UUIDs).
- **Entity Configuration**: Use `IEntityTypeConfiguration<T>` for fluent entity configuration. All entity configurations must reside in `Data/Configurations/` folder.
- **Configuration Discovery**: `ApplyConfigurationsFromAssembly` is used for automatic configuration discovery.
- **Audit Interceptor**: `AuditInterceptor` automatically populates audit fields (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) on save.
