# T3mmyVSA Template

A reusable .NET project template based on the Vertical Slice Architecture.

## Architecture Highlights

- **Framework**: .NET 10 (ASP.NET Core Web API)
- **Pattern**: Vertical Slice Architecture tailored for feature isolation.
- **Database**: SQL Server 2025 with EF Core 10.
- **Logging**: Serilog Structured Logging.
- **Dependency Injection**: Scrutor for automatic assembly scanning.

## Features

- **Vertical Slice Architecture**: Organized by Features (Use Cases) rather than technical layers.
- **REST API**: Built with ASP.NET Core Web API.
- **EF Core 10**: Pre-configured with SQL Server support.
- **Solid Foundation**: Base entities, repository patterns (if used), and robust configuration.
- **API Versioning**: Built-in support.
- **OpenAPI/Swagger**: Verification and testing UI.

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or Container)

### Setup

1. **Install the Template**

   ```bash
   dotnet new install .
   ```

2. **Create a New Project**

   ```bash
   dotnet new t3mmyvsa -n YourProjectName
   ```

3. **Verify**
   Open the solution in your IDE.

### Parameters

| Parameter | Short | Description              | Default                |
| --------- | ----- | ------------------------ | ---------------------- |
| `--name`  | `-n`  | The name of the project. | Current directory name |

## Project Structure

The generated project follows the Vertical Slice Architecture:

- `Features/`: Contains the application logic features.
- `Data/`: Database context and configurations.
- `Entities/`: Domain entities.
- `api/`: API endpoints (organized by feature or via generic controllers).

## Update Template

If you have modified the local template source, you can update the installed version by running:

```bash
dotnet new install --force /Users/t3mmy/Documents/workspaces/aft-workspace/dotnet/e-Haulage
```

(Note: Replace the path with the actual path to your template source if it changes).

## License

[Add your license here]
