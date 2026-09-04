# T3mmyvsa.CLI

Companion scaffolding tool for T3mmyVSA projects.

## Installation

```bash
dotnet tool install -g T3mmyvsa.CLI
```

The project template no longer installs a global CLI automatically. This avoids silently replacing a developer's global tool version. Install or update the CLI explicitly when you want to use scaffolding.

## Commands

### Create an entity

```bash
dotnet t3mmyvsa make:entity Product
```

By default this creates an `AuditableEntity`, its EF Core configuration, and a `DbSet<Product>`.

Use `--base` only when the entity intentionally does not need audit timestamps:

```bash
dotnet t3mmyvsa make:entity LookupValue --base
```

Existing files are not overwritten unless `--force` is supplied.

### Create CRUD vertical slices

```bash
dotnet t3mmyvsa make:feature Product
```

The generator creates:

- `CreateProduct`
- `UpdateProduct`
- `DeleteProduct`
- `GetProduct`
- `GetProducts`
- `BulkDeleteProducts`

Generated slices use:

- `Guid` identifiers to match `BaseEntity`
- Carter endpoints
- Cortex Mediator commands/queries
- FluentValidation
- ProblemDetails-compatible exceptions
- granular `View/Create/Update/Delete` permissions
- bounded pagination with `PaginationRequest`
- deterministic sorting
- cancellation-token propagation
- `AsNoTracking()` on reads
- dynamic request paths for pagination links
- both API versions currently exposed by the starter

The generator will not overwrite an existing feature set unless you explicitly pass `--force`.

```bash
dotnet t3mmyvsa make:feature Product --force
```

Use `--force` only for disposable/generated code. It can replace files you have edited.

## Entity property handling

The CLI scaffolds scalar/value properties declared directly on the entity.

Optional navigation properties and collections are skipped because generic CRUD transport contracts should not expose EF navigation graphs automatically. A required navigation property causes scaffolding to fail so the relationship can be modeled explicitly with a foreign-key/value contract instead of generating unsafe API input.

`init`-only entity properties are included in create/read contracts but are excluded from generated update assignments.

## Naming

Entity names must be valid PascalCase C# identifiers. The CLI applies conservative English pluralization and kebab-case routes, for example:

- `Product` -> `Products` -> `/products`
- `OrderItem` -> `OrderItems` -> `/order-items`
- `Category` -> `Categories` -> `/categories`

Irregular plurals should be adjusted manually after generation.

## Requirements

- .NET 10+
- Run from the generated web project, one of its subdirectories, or a solution directory containing exactly one T3mmyVSA web project.

## License

MIT
