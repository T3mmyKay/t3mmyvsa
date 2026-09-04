namespace T3mmyvsa.CLI.Scaffolding;

internal static partial class FeatureTemplateFactory
{
    private static void AddCreateTemplates(
        ICollection<GeneratedFile> files,
        string rootNamespace,
        string entityName,
        string pluralName,
        string route,
        IReadOnlyList<EntityProperty> properties)
    {
        var featureName = $"Create{entityName}";
        var featureNamespace = NamespaceFor(rootNamespace, pluralName, featureName);
        var commandProperties = RenderProperties(properties);
        var objectInitializer = RenderObjectInitializer(properties, "command");
        var rules = RenderValidationRules(properties);

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}Command.cs",
            $$"""
using {{rootNamespace}}.Models.Shared;

namespace {{featureNamespace}};

public sealed class {{featureName}}Command : ICommand<ResultResponse>
{
{{Indent(commandProperties, 4)}}
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
        var entity = new {{entityName}}
        {
{{Indent(objectInitializer, 12)}}
        };

        context.Set<{{entityName}}>().Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return new ResultResponse
        {
            Id = entity.Id,
            Message = "{{entityName}} created successfully"
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
{{Indent(rules, 8)}}
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
        app.MapPost("{{route}}", async ([FromBody] {{featureName}}Command command, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendCommandAsync<{{featureName}}Command, ResultResponse>(command, ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof({{featureName}}Endpoint))
        .WithTags("{{pluralName}}")
        .WithSummary("Create a new {{entityName}}")
        .Produces<ResultResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermissions(AppPermission.{{Permission(pluralName, "Create")}});
    }
}
"""));
    }

    private static void AddUpdateTemplates(
        ICollection<GeneratedFile> files,
        string rootNamespace,
        string entityName,
        string pluralName,
        string route,
        IReadOnlyList<EntityProperty> properties)
    {
        var featureName = $"Update{entityName}";
        var featureNamespace = NamespaceFor(rootNamespace, pluralName, featureName);
        var updateProperties = properties.Where(property => property.CanUpdate).ToList();
        var commandProperties = RenderProperties(updateProperties);
        var assignments = RenderAssignments(updateProperties, "entity", "command");
        var rules = RenderValidationRules(updateProperties);

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}Command.cs",
            $$"""
using {{rootNamespace}}.Models.Shared;

namespace {{featureNamespace}};

public sealed class {{featureName}}Command : ICommand<ResultResponse>
{
    public Guid Id { get; init; }
{{Indent(commandProperties, 4)}}
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
        var entity = await context.Set<{{entityName}}>()
            .FirstOrDefaultAsync(item => item.Id == command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("{{entityName}} not found.");

{{Indent(assignments, 8)}}

        await context.SaveChangesAsync(cancellationToken);

        return new ResultResponse
        {
            Id = entity.Id,
            Message = "{{entityName}} updated successfully"
        };
    }
}
"""));

        var validationRules =
            "RuleFor(x => x.Id).NotEmpty();" +
            Environment.NewLine +
            rules;

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}CommandValidator.cs",
            $$"""
using FluentValidation;

namespace {{featureNamespace}};

public sealed class {{featureName}}CommandValidator : AbstractValidator<{{featureName}}Command>
{
    public {{featureName}}CommandValidator()
    {
{{Indent(validationRules, 8)}}
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
        app.MapPut("{{route}}/{id:guid}", async ([FromRoute] Guid id, [FromBody] {{featureName}}Command command, IMediator mediator, CancellationToken ct) =>
        {
            if (id != command.Id)
            {
                throw new ArgumentException("Route id must match the request body id.");
            }

            var response = await mediator.SendCommandAsync<{{featureName}}Command, ResultResponse>(command, ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof({{featureName}}Endpoint))
        .WithTags("{{pluralName}}")
        .WithSummary("Update a {{entityName}}")
        .Produces<ResultResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.{{Permission(pluralName, "Update")}});
    }
}
"""));
    }
}
