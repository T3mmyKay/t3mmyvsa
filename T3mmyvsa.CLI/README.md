# T3mmyvsa.CLI

A CLI tool for scaffolding entities and features in T3mmyVSA projects.

## Installation

```bash
dotnet tool install -g T3mmyvsa.CLI
```

## Usage

### Create an Entity

```bash
dotnet t3mmyvsa make:entity User
```

This scaffolds:
- `Entities/User.cs`
- `Configurations/UserConfiguration.cs`
- Updates `AppDbContext.cs`

### Create a Feature

```bash
dotnet t3mmyvsa make:feature User
```

This scaffolds full CRUD:
- `Features/Users/CreateUser/` (Command, Handler, Endpoint, Validator)
- `Features/Users/GetUsers/` (Query, Handler, Endpoint)
- `Features/Users/GetUser/` (Query, Handler, Endpoint)
- `Features/Users/UpdateUser/` (Command, Handler, Endpoint, Validator)
- `Features/Users/DeleteUser/` (Command, Handler, Endpoint)

## Requirements

- .NET 10.0+
- Must be run from a T3mmyVSA project directory

## License

MIT
