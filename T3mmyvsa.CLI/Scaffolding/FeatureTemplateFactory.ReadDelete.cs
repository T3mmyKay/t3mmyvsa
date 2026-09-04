namespace T3mmyvsa.CLI.Scaffolding;

internal static partial class FeatureTemplateFactory
{
    private static void AddDeleteTemplates(
        ICollection<GeneratedFile> files,
        string rootNamespace,
        string entityName,
        string pluralName,
        string route)
    {
        var featureName = $"Delete{entityName}";
        var featureNamespace = NamespaceFor(rootNamespace, pluralName, featureName);

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}Command.cs",
            $$"""
using Microsoft.AspNetCore.Mvc;
using {{rootNamespace}}.Models.Shared;

namespace {{featureNamespace}};

public sealed class {{featureName}}Command : ICommand<ResultResponse>
{
    [FromRoute(Name = "id")]
    public Guid Id { get; init; }
}
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}CommandHandler.cs",
            $$"""
using {{rootNamespace}}.Data;
using {{rootNamespace}}.Entities;
using {{rootNamespace}}.Models.Shared;

namespace {{featureNamespace}};

public sealed class {{featureName}}CommandHandler(AppDbContext context)
    : ICommandHandler<{{featureName}}Command, ResultResponse>
{
    public async Task<ResultResponse> Handle({{featureName}}Command command, CancellationToken cancellationToken)
    {
        if (command.Id == Guid.Empty)
        {
            throw new ArgumentException("Id is required.");
        }

        var entity = await context.Set<{{entityName}}>()
            .FirstOrDefaultAsync(item => item.Id == command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("{{entityName}} not found.");

        context.Set<{{entityName}}>().Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return new ResultResponse
        {
            Id = entity.Id,
            Message = "{{entityName}} deleted successfully"
        };
    }
}
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}CommandValidator.cs",
            $$"""
using FluentValidation;

namespace {{featureNamespace}};

public sealed class {{featureName}}CommandValidator : AbstractValidator<{{featureName}}Command>
{
    public {{featureName}}CommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}Endpoint.cs",
            $$"""
using {{rootNamespace}}.Authorization.Enums;
using {{rootNamespace}}.Extensions;
using {{rootNamespace}}.Models.Shared;

namespace {{featureNamespace}};

public sealed class {{featureName}}Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("{{route}}/{id:guid}", async ([AsParameters] {{featureName}}Command command, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendCommandAsync<{{featureName}}Command, ResultResponse>(command, ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof({{featureName}}Endpoint))
        .WithTags("{{pluralName}}")
        .WithSummary("Delete a {{entityName}}")
        .Produces<ResultResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.{{Permission(pluralName, "Delete")}});
    }
}
"""));
    }

    private static void AddGetTemplates(
        ICollection<GeneratedFile> files,
        string rootNamespace,
        string entityName,
        string pluralName,
        string route,
        IReadOnlyList<EntityProperty> properties,
        bool isAuditable)
    {
        var featureName = $"Get{entityName}";
        var featureNamespace = NamespaceFor(rootNamespace, pluralName, featureName);
        var responseProperties = RenderResponseProperties(properties, isAuditable);
        var responseAssignments = RenderResponseAssignments(properties, isAuditable, "entity");

        files.Add(new GeneratedFile(
            $"{featureName}/{entityName}Response.cs",
            $$"""
namespace {{featureNamespace}};

public sealed class {{entityName}}Response
{
    public Guid Id { get; init; }
{{Indent(responseProperties, 4)}}
}
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}Query.cs",
            $$"""
using Microsoft.AspNetCore.Mvc;

namespace {{featureNamespace}};

public sealed class {{featureName}}Query : IQuery<{{entityName}}Response>
{
    [FromRoute(Name = "id")]
    public Guid Id { get; init; }
}
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}QueryHandler.cs",
            $$"""
using {{rootNamespace}}.Data;
using {{rootNamespace}}.Entities;

namespace {{featureNamespace}};

public sealed class {{featureName}}QueryHandler(AppDbContext context)
    : IQueryHandler<{{featureName}}Query, {{entityName}}Response>
{
    public async Task<{{entityName}}Response> Handle({{featureName}}Query query, CancellationToken cancellationToken)
    {
        if (query.Id == Guid.Empty)
        {
            throw new ArgumentException("Id is required.");
        }

        var entity = await context.Set<{{entityName}}>()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == query.Id, cancellationToken)
            ?? throw new KeyNotFoundException("{{entityName}} not found.");

        return new {{entityName}}Response
        {
            Id = entity.Id,
{{Indent(responseAssignments, 12)}}
        };
    }
}
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}QueryValidator.cs",
            $$"""
using FluentValidation;

namespace {{featureNamespace}};

public sealed class {{featureName}}QueryValidator : AbstractValidator<{{featureName}}Query>
{
    public {{featureName}}QueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}Endpoint.cs",
            $$"""
using {{rootNamespace}}.Authorization.Enums;
using {{rootNamespace}}.Extensions;

namespace {{featureNamespace}};

public sealed class {{featureName}}Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("{{route}}/{id:guid}", async ([AsParameters] {{featureName}}Query query, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<{{featureName}}Query, {{entityName}}Response>(query, ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof({{featureName}}Endpoint))
        .WithTags("{{pluralName}}")
        .WithSummary("Get a {{entityName}} by id")
        .Produces<{{entityName}}Response>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.{{Permission(pluralName, "View")}});
    }
}
"""));
    }
}
