# T3mmyVSA Templates

Project templates for building Web APIs using Vertical Slice Architecture.

## Installation

```bash
dotnet new install T3mmy.VSA.Templates
```

## Usage

Create a new project:

```bash
dotnet new t3mmyvsa -n MyProject
```

## What's Included

- **Vertical Slice Architecture** - Feature-based organization
- **Carter** - Minimal API endpoints
- **Cortex.Mediator** - CQRS pattern
- **Entity Framework Core** - Data access
- **FluentValidation** - Request validation
- **Serilog** - Structured logging
- **API Versioning** - Versioned endpoints
- **JWT Authentication** - Secure authentication
- **Role-Based Authorization** - Granular permissions

## Project Structure

```
Features/
├── Users/
│   ├── CreateUser/
│   ├── GetUsers/
│   ├── GetUser/
│   └── ...
Authorization/
Entities/
Configurations/
```

## CLI Tool

Install the companion CLI for scaffolding:

```bash
dotnet tool install -g T3mmyvsa.CLI
```

## License

MIT
