using System.Text;
using System.Text.RegularExpressions;

if (args.Length == 0)
{
    PrintUsage();
    return;
}

var command = args[0].ToLower();

try
{
    switch (command)
    {
        case "createentity": // Legacy support
        case "make:entity":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Entity name is required.");
                PrintUsage();
                return;
            }
            CreateEntity(args[1]);
            break;

        case "make:feature":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Entity name is required.");
                PrintUsage();
                return;
            }
            CreateFeature(args[1]);
            break;

        default:
            Console.WriteLine($"Unknown command: {command}");
            PrintUsage();
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet t3mmyvsa make:entity <EntityName>");
    Console.WriteLine("  dotnet t3mmyvsa make:feature <EntityName>");
}

void CreateEntity(string entityName)
{
    // Ensure we are in the project root (where Entities and Data folders exist)
    var currentDir = Directory.GetCurrentDirectory();
    var rootDir = FindProjectRoot(currentDir);

    if (rootDir == null)
    {
        Console.WriteLine("Error: Could not find project root (looking for Entities and Data folders).");
        return;
    }

    var rootNamespace = GetRootNamespace(rootDir);
    Console.WriteLine($"Scaffolding Entity: {entityName} in {rootDir} (Namespace: {rootNamespace})");

    var entitiesDir = Path.Combine(rootDir, "Entities");
    var dataDir = Path.Combine(rootDir, "Data");
    var configurationsDir = Path.Combine(dataDir, "Configurations");

    // 1. Create Entity
    var entityContent = $@"using {rootNamespace}.Entities.Base;

namespace {rootNamespace}.Entities;

public class {entityName} : BaseEntity
{{
}}";

    var entityPath = Path.Combine(entitiesDir, $"{entityName}.cs");
    if (!Directory.Exists(entitiesDir)) Directory.CreateDirectory(entitiesDir);
    if (!File.Exists(entityPath))
    {
        File.WriteAllText(entityPath, entityContent);
        Console.WriteLine($"Created Entities/{entityName}.cs");
    }
    else
    {
        Console.WriteLine($"Entity {entityName} already exists. Skipping creation.");
    }

    // 2. Create Configuration
    var configContent = $@"using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {rootNamespace}.Entities;

namespace {rootNamespace}.Data.Configurations;

public class {entityName}Configuration : IEntityTypeConfiguration<{entityName}>
{{
    public void Configure(EntityTypeBuilder<{entityName}> builder)
    {{
        builder.ToTable(""{entityName}s"");
    }}
}}";

    var configPath = Path.Combine(configurationsDir, $"{entityName}Configuration.cs");
    if (!Directory.Exists(configurationsDir)) Directory.CreateDirectory(configurationsDir);
    if (!File.Exists(configPath))
    {
        File.WriteAllText(configPath, configContent);
        Console.WriteLine($"Created Data/Configurations/{entityName}Configuration.cs");
    }
    else
    {
        Console.WriteLine($"Configuration for {entityName} already exists. Skipping creation.");
    }

    // 3. Update AppDbContext.cs
    var contextPath = Path.Combine(dataDir, "AppDbContext.cs");
    if (File.Exists(contextPath))
    {
        var lines = File.ReadAllLines(contextPath).ToList();
        var searchPattern = "DbSet<AuditLog> AuditLogs";
        var insertLine = $"    public DbSet<{entityName}> {entityName}s {{ get; set; }}";

        bool found = false;
        bool alreadyExists = false;

        // Check if DbSet already exists
        if (lines.Any(l => l.Contains($"DbSet<{entityName}>")))
        {
            alreadyExists = true;
        }

        if (!alreadyExists)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(searchPattern))
                {
                    lines.Insert(i + 1, insertLine);
                    found = true;
                    break;
                }
            }

            if (found)
            {
                File.WriteAllLines(contextPath, lines);
                Console.WriteLine("Updated Data/AppDbContext.cs");
            }
            else
            {
                Console.WriteLine("Warning: Could not find AuditLog DbSet in AppDbContext.cs. Please add DbSet manually.");
            }
        }
        else
        {
            Console.WriteLine($"DbSet for {entityName} already exists in AppDbContext.cs. Skipping update.");
        }
    }
    else
    {
        Console.WriteLine("Error: AppDbContext.cs not found.");
    }

    Console.WriteLine("Done.");
}

void CreateFeature(string entityName)
{
    Console.WriteLine($"Scaffolding Feature for: {entityName}");

    var currentDir = Directory.GetCurrentDirectory();
    var rootDir = FindProjectRoot(currentDir);

    if (rootDir == null)
    {
        Console.WriteLine("Error: Could not find project root.");
        return;
    }

    var rootNamespace = GetRootNamespace(rootDir);
    Console.WriteLine($"Detected Root Namespace: {rootNamespace}");

    var entityPath = Path.Combine(rootDir, "Entities", $"{entityName}.cs");
    if (!File.Exists(entityPath))
    {
        Console.WriteLine($"Error: Entity file not found at {entityPath}. Please create the entity first.");
        return;
    }

    // Parse Entity Properties Properties
    var properties = ParseEntityProperties(entityPath);
    if (properties.Count == 0)
    {
        Console.WriteLine("Warning: No public properties found in entity. Scaffolding with Id only.");
    }

    // Define paths
    var featuresDir = Path.Combine(rootDir, "Features", $"{entityName}s"); // Pluralize roughly

    // Create Features Directory
    if (!Directory.Exists(featuresDir)) Directory.CreateDirectory(featuresDir);

    // 1. Create Feature Base
    CreateCreateFeature(rootDir, featuresDir, entityName, properties, rootNamespace);
    CreateUpdateFeature(rootDir, featuresDir, entityName, properties, rootNamespace);
    CreateDeleteFeature(rootDir, featuresDir, entityName, rootNamespace);
    CreateGetFeature(rootDir, featuresDir, entityName, properties, rootNamespace);
    CreateGetListFeature(rootDir, featuresDir, entityName, properties, rootNamespace);
    CreateBulkDeleteFeature(rootDir, featuresDir, entityName, rootNamespace);

    // 2. Add Permissions
    AddPermissions(rootDir, entityName, rootNamespace);

    Console.WriteLine($"Feature scaffolding complete for {entityName}. Check {featuresDir}");
}

List<(string Type, string Name)> ParseEntityProperties(string entityPath)
{
    var properties = new List<(string Type, string Name)>();
    var lines = File.ReadAllLines(entityPath);

    // Simple regex to match: public string Name { get; set; }
    var propRegex = new Regex(@"public\s+([a-zA-Z0-9_<>?]+)\s+([a-zA-Z0-9_]+)\s*\{\s*get;\s*set;\s*\}");

    foreach (var line in lines)
    {
        var match = propRegex.Match(line);
        if (match.Success)
        {
            var type = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            if (name != "Id" && name != "CreatedAt" && name != "UpdatedAt" && name != "CreatedBy" && name != "UpdatedBy")
            {
                properties.Add((type, name));
            }
        }
    }
    return properties;
}

void CreateCreateFeature(string rootDir, string featuresDir, string entityName, List<(string Type, string Name)> props, string rootNamespace)
{
    var featureName = $"Create{entityName}";
    var dir = Path.Combine(featuresDir, featureName);
    Directory.CreateDirectory(dir);

    // Command
    var propsList = string.Join(",\n    ", props.Select(p => $"public {p.Type} {p.Name} {{ get; set; }}"));
    var commandContent = $@"using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Command : ICommand<ResultResponse>
{{
    {propsList}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Command.cs"), commandContent);

    // Handler
    var assignments = string.Join("\n        ", props.Select(p => $"entity.{p.Name} = command.{p.Name};"));
    var handlerContent = $@"using {rootNamespace}.Entities;
using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Handler(AppDbContext context) : ICommandHandler<{featureName}Command, ResultResponse>
{{
    public async Task<ResultResponse> Handle({featureName}Command command, CancellationToken cancellationToken)
    {{
        var entity = new {entityName}();
        {assignments}
        
        context.{entityName}s.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return new ResultResponse {{ Id = entity.Id, Message = ""{entityName} created successfully"", Success = true }};
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Handler.cs"), handlerContent);

    // Endpoint
    var endpointContent = $@"using {rootNamespace}.Authorization.Enums;
using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Endpoint : ICarterModule
{{
    public void AddRoutes(IEndpointRouteBuilder app)
    {{
        app.MapPost(""{entityName.ToLower()}s"", async ({featureName}Command command, IMediator mediator, CancellationToken ct) =>
        {{
            var response = await mediator.SendCommandAsync<{featureName}Command, ResultResponse>(command, ct);
            return Results.Ok(response);
        }})
        .HasApiVersion(1)
        .WithName(nameof({featureName}Endpoint))
        .WithTags(""{entityName}s"")
        .WithSummary(""Create a new {entityName}"")
        .Produces<ResultResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .HasPermissions(AppPermission.{entityName}sCreate);
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Endpoint.cs"), endpointContent);

    // Validator
    var rules = string.Join("\n        ", props.Select(p =>
        p.Type == "string" ? $"RuleFor(x => x.{p.Name}).NotEmpty();" : ""));
    var validatorContent = $@"
namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Validator : AbstractValidator<{featureName}Command>
{{
    public {featureName}Validator()
    {{
        {rules}
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Validator.cs"), validatorContent);
}

void CreateUpdateFeature(string rootDir, string featuresDir, string entityName, List<(string Type, string Name)> props, string rootNamespace)
{
    var featureName = $"Update{entityName}";
    var dir = Path.Combine(featuresDir, featureName);
    Directory.CreateDirectory(dir);

    // Command
    var propsList = string.Join(",\n    ", props.Select(p => $"public {p.Type} {p.Name} {{ get; set; }}"));
    var commandContent = $@"using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Command : ICommand<ResultResponse>
{{
    public string Id {{ get; set; }}
    {propsList}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Command.cs"), commandContent);

    // Handler
    var assignments = string.Join("\n        ", props.Select(p => $"entity.{p.Name} = command.{p.Name};"));
    var handlerContent = $@"using {rootNamespace}.Entities;
using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Handler(AppDbContext context) : ICommandHandler<{featureName}Command, ResultResponse>
{{
    public async Task<ResultResponse> Handle({featureName}Command command, CancellationToken cancellationToken)
    {{
        var entity = await context.{entityName}s.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException(""{entityName} not found"");

        {assignments}
        
        context.{entityName}s.Update(entity);
        await context.SaveChangesAsync(cancellationToken);

        return new ResultResponse {{ Id = entity.Id, Message = ""{entityName} updated successfully"", Success = true }};
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Handler.cs"), handlerContent);

    // Endpoint
    var endpointContent = $@"using {rootNamespace}.Authorization.Enums;
using {rootNamespace}.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Endpoint : ICarterModule
{{
    public void AddRoutes(IEndpointRouteBuilder app)
    {{
        app.MapPut(""{entityName.ToLower()}s/{{id}}"", async ([FromRoute] string id, [FromBody] {featureName}Command command, IMediator mediator, CancellationToken ct) =>
        {{
            if (id != command.Id) return Results.BadRequest(""Id mismatch"");
            var response = await mediator.SendCommandAsync<{featureName}Command, ResultResponse>(command, ct);
            return Results.Ok(response);
        }})
        .HasApiVersion(1)
        .WithName(nameof({featureName}Endpoint))
        .WithTags(""{entityName}s"")
        .WithSummary(""Update a {entityName}"")
        .Produces<ResultResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.{entityName}sUpdate);
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Endpoint.cs"), endpointContent);

    // Validator
    var rules = string.Join("\n        ", props.Select(p =>
        p.Type == "string" ? $"RuleFor(x => x.{p.Name}).NotEmpty();" : ""));
    var validatorContent = $@"
namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Validator : AbstractValidator<{featureName}Command>
{{
    public {featureName}Validator()
    {{
        RuleFor(x => x.Id).NotEmpty();
        {rules}
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Validator.cs"), validatorContent);
}

void CreateDeleteFeature(string rootDir, string featuresDir, string entityName, string rootNamespace)
{
    var featureName = $"Delete{entityName}";
    var dir = Path.Combine(featuresDir, featureName);
    Directory.CreateDirectory(dir);

    // Command
    var commandContent = $@"using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public record {featureName}Command(string Id) : ICommand<ResultResponse>;
";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Command.cs"), commandContent);

    // Handler
    var handlerContent = $@"using {rootNamespace}.Entities;
using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Handler(AppDbContext context) : ICommandHandler<{featureName}Command, ResultResponse>
{{
    public async Task<ResultResponse> Handle({featureName}Command command, CancellationToken cancellationToken)
    {{
        var entity = await context.{entityName}s.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException(""{entityName} not found"");

        context.{entityName}s.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return new ResultResponse {{ Id = entity.Id, Message = ""{entityName} deleted successfully"", Success = true }};
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Handler.cs"), handlerContent);

    // Endpoint
    var endpointContent = $@"using {rootNamespace}.Authorization.Enums;
using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Endpoint : ICarterModule
{{
    public void AddRoutes(IEndpointRouteBuilder app)
    {{
        app.MapDelete(""{entityName.ToLower()}s/{{id}}"", async (string id, IMediator mediator, CancellationToken ct) =>
        {{
            var response = await mediator.SendCommandAsync<{featureName}Command, ResultResponse>(new {featureName}Command(id), ct);
            return Results.Ok(response);
        }})
        .HasApiVersion(1)
        .WithName(nameof({featureName}Endpoint))
        .WithTags(""{entityName}s"")
        .WithSummary(""Delete a {entityName}"")
        .Produces<ResultResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        // reusing Create implies Manage access
        .HasPermissions(AppPermission.{entityName}sCreate); 
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Endpoint.cs"), endpointContent);
}

void CreateGetFeature(string rootDir, string featuresDir, string entityName, List<(string Type, string Name)> props, string rootNamespace)
{
    var featureName = $"Get{entityName}";
    var dir = Path.Combine(featuresDir, featureName);
    Directory.CreateDirectory(dir);

    // Response DTO
    var propsList = string.Join(",\n    ", props.Select(p => $"public {p.Type} {p.Name} {{ get; set; }}"));
    var responseContent = $@"
namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {entityName}Response 
{{
    public string Id {{ get; set; }}
    {propsList}
}}";
    File.WriteAllText(Path.Combine(dir, $"{entityName}Response.cs"), responseContent);

    // Query
    var queryContent = $@"
namespace {rootNamespace}.Features.{entityName}s.{featureName};

public record {featureName}Query(string Id) : IQuery<{entityName}Response>;
";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Query.cs"), queryContent);

    // Handler
    var assignments = string.Join("\n            ", props.Select(p => $"{p.Name} = entity.{p.Name},"));
    var handlerContent = $@"using {rootNamespace}.Entities;
using Riok.Mapperly.Abstractions;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Handler(AppDbContext context) : IQueryHandler<{featureName}Query, {entityName}Response>
{{
    public async Task<{entityName}Response> Handle({featureName}Query query, CancellationToken cancellationToken)
    {{
        var entity = await context.{entityName}s.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException(""{entityName} not found"");

        return new {entityName}Response
        {{
            Id = entity.Id,
            {assignments}
        }};
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Handler.cs"), handlerContent);

    // Endpoint
    var endpointContent = $@"using {rootNamespace}.Authorization.Enums;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Endpoint : ICarterModule
{{
    public void AddRoutes(IEndpointRouteBuilder app)
    {{
        app.MapGet(""{entityName.ToLower()}s/{{id}}"", async (string id, IMediator mediator, CancellationToken ct) =>
        {{
            var response = await mediator.SendQueryAsync<{featureName}Query, {entityName}Response>(new {featureName}Query(id), ct);
            return Results.Ok(response);
        }})
        .HasApiVersion(1)
        .WithName(nameof({featureName}Endpoint))
        .WithTags(""{entityName}s"")
        .WithSummary(""Get a {entityName} by Id"")
        .Produces<{entityName}Response>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.{entityName}sView);
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Endpoint.cs"), endpointContent);
}

void CreateGetListFeature(string rootDir, string featuresDir, string entityName, List<(string Type, string Name)> props, string rootNamespace)
{
    var featureName = $"Get{entityName}s";
    var dir = Path.Combine(featuresDir, featureName);
    Directory.CreateDirectory(dir);

    var propsList = string.Join(",\n    ", props.Select(p => $"public {p.Type} {p.Name} {{ get; set; }}"));
    var responseContent = $@"
namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {entityName}ListResponse 
{{
    public string Id {{ get; set; }}
    {propsList}
}}";
    File.WriteAllText(Path.Combine(dir, $"{entityName}ListResponse.cs"), responseContent);

    // Request
    var requestContent = $@"using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Request : PagedRequest
{{
    public string? Search {{ get; set; }}
    public string? SortColumn {{ get; set; }}
    public SortOrder SortOrder {{ get; set; }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Request.cs"), requestContent);


    // Query
    var queryContent = $@"using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public record {featureName}Query({featureName}Request Request) : IQuery<PaginatedResponse<{entityName}ListResponse>>;
";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Query.cs"), queryContent);

    // Handler
    var assignments = string.Join("\n            ", props.Select(p => $"{p.Name} = x.{p.Name},"));
    var handlerContent = $@"using {rootNamespace}.Entities;
using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Handler(AppDbContext context) : IQueryHandler<{featureName}Query, PaginatedResponse<{entityName}ListResponse>>
{{
    public async Task<PaginatedResponse<{entityName}ListResponse>> Handle({featureName}Query query, CancellationToken cancellationToken)
    {{
        var request = query.Request;
        var queryable = context.{entityName}s.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {{
             // queryable = queryable.Where(x => x.Id.Contains(request.Search)); 
        }}

        // Simple sorting
        queryable = request.SortOrder == SortOrder.Desc 
            ? queryable.OrderByDescending(x => x.CreatedAt) 
            : queryable.OrderBy(x => x.CreatedAt);

        var responseQuery = queryable.Select(x => new {entityName}ListResponse
        {{
            Id = x.Id,
            {assignments}
        }});

        var pagedList = await PagedList<{entityName}ListResponse>.CreateAsync(responseQuery, request.Page ?? 1, request.PageSize ?? 15);
        
         var userResponses = pagedList.ToList();
         var meta = new PaginationMeta
        {{
            CurrentPage = pagedList.CurrentPage,
            From = pagedList.TotalCount == 0 ? 0 : (pagedList.CurrentPage - 1) * pagedList.PageSize + 1,
            LastPage = pagedList.TotalPages,
            Path = ""/api/v1/{entityName.ToLower()}s"",
            PerPage = pagedList.PageSize,
            To = pagedList.TotalCount == 0 ? 0 : (pagedList.CurrentPage - 1) * pagedList.PageSize + userResponses.Count,
            Total = pagedList.TotalCount
        }};

         return new PaginatedResponse<{entityName}ListResponse>(userResponses, meta, null);
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Handler.cs"), handlerContent);

    // Endpoint
    var endpointContent = $@"using {rootNamespace}.Authorization.Enums;
using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Endpoint : ICarterModule
{{
    public void AddRoutes(IEndpointRouteBuilder app)
    {{
        app.MapGet(""{entityName.ToLower()}s"", async ([AsParameters] {featureName}Request request, IMediator mediator, CancellationToken ct) =>
        {{
            var response = await mediator.SendQueryAsync<{featureName}Query, PaginatedResponse<{entityName}ListResponse>>(new {featureName}Query(request), ct);
            return Results.Ok(response);
        }})
        .HasApiVersion(1)
        .WithName(nameof({featureName}Endpoint))
        .WithTags(""{entityName}s"")
        .WithSummary(""Get all {entityName}s"")
        .Produces<PaginatedResponse<{entityName}ListResponse>>(StatusCodes.Status200OK)
        .HasPermissions(AppPermission.{entityName}sView);
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Endpoint.cs"), endpointContent);
}

void CreateBulkDeleteFeature(string rootDir, string featuresDir, string entityName, string rootNamespace)
{
    var featureName = $"BulkDelete{entityName}s";
    var dir = Path.Combine(featuresDir, featureName);
    Directory.CreateDirectory(dir);

    // Command
    var commandContent = $@"using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public record {featureName}Command(List<string> Ids) : ICommand<ResultResponse>;
";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Command.cs"), commandContent);

    // Handler
    var handlerContent = $@"using {rootNamespace}.Entities;
using {rootNamespace}.Models.Shared;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Handler(AppDbContext context) : ICommandHandler<{featureName}Command, ResultResponse>
{{
    public async Task<ResultResponse> Handle({featureName}Command command, CancellationToken cancellationToken)
    {{
        var entities = await context.{entityName}s
            .Where(x => command.Ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
             return new ResultResponse {{ Message = ""No entities found to delete"", Success = false }};

        context.{entityName}s.RemoveRange(entities);
        await context.SaveChangesAsync(cancellationToken);

        return new ResultResponse {{ Message = $""{{entities.Count}} {entityName}s deleted successfully"", Success = true }};
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Handler.cs"), handlerContent);

    // Endpoint
    var endpointContent = $@"using {rootNamespace}.Authorization.Enums;
using {rootNamespace}.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace {rootNamespace}.Features.{entityName}s.{featureName};

public class {featureName}Endpoint : ICarterModule
{{
    public void AddRoutes(IEndpointRouteBuilder app)
    {{
        app.MapDelete(""{entityName.ToLower()}s/bulk"", async ([FromBody] {featureName}Command command, IMediator mediator, CancellationToken ct) =>
        {{
            var response = await mediator.SendCommandAsync<{featureName}Command, ResultResponse>(command, ct);
            return Results.Ok(response);
        }})
        .HasApiVersion(1)
        .WithName(nameof({featureName}Endpoint))
        .WithTags(""{entityName}s"")
        .WithSummary(""Bulk delete {entityName}s"")
        .Produces<ResultResponse>(StatusCodes.Status200OK)
        .HasPermissions(AppPermission.{entityName}sCreate); // reusing Create implies Manage
    }}
}}";
    File.WriteAllText(Path.Combine(dir, $"{featureName}Endpoint.cs"), endpointContent);
}

void AddPermissions(string rootDir, string entityName, string rootNamespace)
{
    var permissionFile = Path.Combine(rootDir, "Authorization", "Enums", "AppPermission.cs");
    if (!File.Exists(permissionFile))
    {
        Console.WriteLine("Warning: AppPermission.cs not found, cannot add permissions.");
        return;
    }

    var lines = File.ReadAllLines(permissionFile).ToList();
    var newPermissions = new List<(string Name, string Description)>
    {
        ($"{entityName}sView", $"{entityName}s.View"),
        ($"{entityName}sCreate", $"{entityName}s.Create"),
        ($"{entityName}sUpdate", $"{entityName}s.Update"),
    };

    var lastEnumLineIndex = lines.FindLastIndex(l => l.Trim().EndsWith("}"));
    // Insert before the last closing brace
    if (lastEnumLineIndex != -1)
    {
        bool addedAny = false;
        var insertionIndex = lastEnumLineIndex;

        // Check if there are new permissions to add
        var toAdd = newPermissions.Where(p => !lines.Any(l => l.Contains(p.Name))).ToList();

        if (toAdd.Count > 0)
        {
            // Ensure previous item has comma
            var prevIndex = insertionIndex - 1;
            while (prevIndex >= 0 && string.IsNullOrWhiteSpace(lines[prevIndex]))
            {
                prevIndex--;
            }

            if (prevIndex >= 0)
            {
                var prevLine = lines[prevIndex].TrimEnd();
                // Check if it's a valid enum entry line (not the opening brace or empty)
                if (!prevLine.EndsWith(",") && !prevLine.EndsWith("{"))
                {
                    lines[prevIndex] = lines[prevIndex] + ",";
                }
            }

            foreach (var perm in toAdd)
            {
                lines.Insert(insertionIndex++, "");
                lines.Insert(insertionIndex++, $"    [Description(\"{perm.Description}\")]");
                lines.Insert(insertionIndex++, $"    {perm.Name},");
                addedAny = true;
            }

            if (addedAny)
            {
                File.WriteAllLines(permissionFile, lines);
                Console.WriteLine("Added permissions to AppPermission.cs");
            }
        }
    }
}

string GetRootNamespace(string rootDir)
{
    // Try to find a .csproj file in the rootDir
    var csprojFiles = Directory.GetFiles(rootDir, "*.csproj");
    if (csprojFiles.Length > 0)
    {
        // First, check if RootNamespace is explicitly defined in any of them
        foreach (var csproj in csprojFiles)
        {
            var content = File.ReadAllText(csproj);
            var match = Regex.Match(content, @"<RootNamespace>(.*?)</RootNamespace>");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }

        // Fallback: use the file name of the first csproj found
        return Path.GetFileNameWithoutExtension(csprojFiles[0]);
    }

    // Fallback if no csproj found
    return "T3mmyvsa";
}

string? FindProjectRoot(string startDir)
{
    // 1. Check current directory
    if (Directory.Exists(Path.Combine(startDir, "Entities")) && Directory.Exists(Path.Combine(startDir, "Data")))
        return startDir;

    // 2. Check subdirectories (one level deep) - useful for templates where csproj is in a folder
    var subDirs = Directory.GetDirectories(startDir);
    foreach (var subDir in subDirs)
    {
        if (Directory.Exists(Path.Combine(subDir, "Entities")) && Directory.Exists(Path.Combine(subDir, "Data")))
            return subDir;
    }

    // 3. Optional: Traverse up (if we were deep inside)
    var dir = new DirectoryInfo(startDir).Parent;
    while (dir != null)
    {
        if (dir.GetDirectories("Entities").Any() && dir.GetDirectories("Data").Any())
        {
            return dir.FullName;
        }
        dir = dir.Parent;
    }

    return null;
}
