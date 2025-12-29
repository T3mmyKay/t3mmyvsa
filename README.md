# T3mmyVSA Template

A reusable .NET project template based on the Vertical Slice Architecture.

## Architecture Highlights

- **Framework**: .NET 10 (ASP.NET Core Web API)
- **Pattern**: Vertical Slice Architecture tailored for feature isolation.
- **Database**: SQL Server 2022 with EF Core 10.
- **Logging**: Serilog Structured Logging.
- **Dependency Injection**: Scrutor for automatic assembly scanning.

## Features

- **Vertical Slice Architecture**: Organized by Features (Use Cases) rather than technical layers.
- **REST API**: Built with ASP.NET Core Web API.
- **EF Core 10**: Pre-configured with SQL Server support.
- **Solid Foundation**: Base entities, repository patterns (if used), and robust configuration.
- **API Versioning**: Built-in support.
- **OpenAPI/Scalar**: Verification and testing UI (Scalar).
- **Background Jobs**: TickerQ for high-performance, reflection-free distributed job scheduling.
- **CLI Tooling**: Dedicated CLI (`dotnet t3mmyvsa`) for scaffolding entities and features involving full CRUD operations.

## Gets Started

### Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or Container)

### Setup

1. **Install the Template**

   ```bash
   dotnet new install ./T3mmyvsa.Web
   ```

2. **Create a New Project**

   ```bash
   dotnet new t3mmyvsa -n YourProjectName
   ```

3. **Install the CLI Tool** (Recommended)

   ```bash
   dotnet tool install -g T3mmyvsa.CLI
   ```

4. **Verify**
   Open the solution in your IDE.

### Parameters

| Parameter | Short | Description              | Default                |
| --------- | ----- | ------------------------ | ---------------------- |
| `--name`  | `-n`  | The name of the project. | Current directory name |

## CLI Tool Usage

The template includes a powerful CLI tool to accelerate development.

### Create an Entity
Scaffolds a new entity, configuration, and updates the DbContext.

```bash
dotnet t3mmyvsa make:entity YourEntityName
```

### Create a Feature
Scaffolds a full set of CRUD features (Create, Update, Delete, Get, GetList, BulkDelete) for an existing entity.

```bash
dotnet t3mmyvsa make:feature YourEntityName
```

## Project Structure

The generated project follows the Vertical Slice Architecture:

- `Features/`: Contains the application logic features (sliced by domain).
- `Data/`: Database context and configurations.
- `Entities/`: Domain entities.
- `Authorization/`: Permissions and policies.

## Background Jobs

This template uses [TickerQ](https://github.com/T3mmyKay/TicketQ) for efficient, reflection-free background job processing.

### Key Features

- **High Performance**: Uses source generators to avoid runtime reflection overhead.
- **Dashboard**: Built-in real-time dashboard for monitoring jobs.
- **EF Core Integration**: Seamlessly stores job state in your database.

### Usage

Define a job using the `[TickerFunction]` attribute:

```csharp
public partial class MyJob
{
    [TickerFunction("DailyCleanup", "0 0 * * *")]
    public async Task CleanupAsync(CancellationToken cancellationToken)
    {
        // Job logic here
    }
}
```

## Update Template

If you have modified the local template source, you can update the installed version by running:

```bash
dotnet new install --force ./T3mmyvsa.Web
```

(Note: Replace `./T3mmyvsa.Web` with the actual path to your template source if you are running this from a different location).

## License

Distributed under the MIT License. See `LICENSE` for more information.

