namespace T3mmyvsa.CLI.Scaffolding;

internal static partial class FeatureTemplateFactory
{
    private static void AddListTemplates(
        ICollection<GeneratedFile> files,
        string rootNamespace,
        string entityName,
        string pluralName,
        string route,
        IReadOnlyList<EntityProperty> properties,
        bool isAuditable)
    {
        var featureName = $"Get{pluralName}";
        var featureNamespace = NamespaceFor(rootNamespace, pluralName, featureName);
        var sortColumnName = $"{entityName}SortColumn";
        var responseProperties = RenderResponseProperties(properties, isAuditable);
        var responseAssignments = RenderResponseAssignments(properties, isAuditable, "entity");
        var stringSearch = RenderSearch(properties);
        var sortableProperties = properties.Where(IsSortable).ToList();
        var defaultSort = isAuditable ? "CreatedAt" : "Id";

        var sortEnumValues = new List<string> { "Id" };
        if (isAuditable)
        {
            sortEnumValues.Add("CreatedAt");
        }

        sortEnumValues.AddRange(sortableProperties.Select(property => property.Name));
        sortEnumValues = sortEnumValues.Distinct(StringComparer.Ordinal).ToList();

        var ascendingCases = RenderSortCases(
            sortableProperties,
            isAuditable,
            descending: false,
            sortColumnName);

        var descendingCases = RenderSortCases(
            sortableProperties,
            isAuditable,
            descending: true,
            sortColumnName);

        files.Add(new GeneratedFile(
            $"{featureName}/{entityName}ListResponse.cs",
            $$"""
namespace {{featureNamespace}};

public sealed class {{entityName}}ListResponse
{
    public Guid Id { get; init; }
{{Indent(responseProperties, 4)}}
}
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{sortColumnName}.cs",
            $$"""
namespace {{featureNamespace}};

public enum {{sortColumnName}}
{
{{Indent(string.Join("," + Environment.NewLine, sortEnumValues), 4)}}
}
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}Request.cs",
            $$"""
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using {{rootNamespace}}.Models.Shared;

namespace {{featureNamespace}};

public sealed class {{featureName}}Request : PaginationRequest
{
    [FromQuery(Name = "search")]
    public string? Search { get; init; }

    [FromQuery(Name = "sort_column")]
    [DefaultValue({{sortColumnName}}.{{defaultSort}})]
    public {{sortColumnName}} SortColumn { get; init; } = {{sortColumnName}}.{{defaultSort}};

    [FromQuery(Name = "sort_order")]
    [DefaultValue(SortOrder.Asc)]
    public SortOrder SortOrder { get; init; } = SortOrder.Asc;
}
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}RequestValidator.cs",
            $$"""
using FluentValidation;
using {{rootNamespace}}.Models.Shared;

namespace {{featureNamespace}};

public sealed class {{featureName}}RequestValidator : AbstractValidator<{{featureName}}Request>
{
    public {{featureName}}RequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).When(x => x.Page.HasValue);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
        RuleFor(x => x.Search).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Search));
        RuleFor(x => x.SortColumn).IsInEnum();
        RuleFor(x => x.SortOrder).IsInEnum();
    }
}
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}Query.cs",
            $$"""
using {{rootNamespace}}.Models.Shared;

namespace {{featureNamespace}};

public sealed record {{featureName}}Query({{featureName}}Request Request)
    : IQuery<PaginatedResponse<{{entityName}}ListResponse>>;
"""));

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}QueryHandler.cs",
            $$"""
using {{rootNamespace}}.Data;
using {{rootNamespace}}.Entities;
using {{rootNamespace}}.Models.Shared;

namespace {{featureNamespace}};

public sealed class {{featureName}}QueryHandler(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor)
    : IQueryHandler<{{featureName}}Query, PaginatedResponse<{{entityName}}ListResponse>>
{
    public async Task<PaginatedResponse<{{entityName}}ListResponse>> Handle(
        {{featureName}}Query query,
        CancellationToken cancellationToken)
    {
        var request = query.Request;
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 15;
        var queryable = context.Set<{{entityName}}>().AsNoTracking();

{{Indent(stringSearch, 8)}}

        queryable = request.SortOrder == SortOrder.Desc
            ? request.SortColumn switch
            {
{{Indent(descendingCases, 16)}}
            }
            : request.SortColumn switch
            {
{{Indent(ascendingCases, 16)}}
            };

        var responseQuery = queryable.Select(entity => new {{entityName}}ListResponse
        {
            Id = entity.Id,
{{Indent(responseAssignments, 12)}}
        });

        var pagedList = await PagedList<{{entityName}}ListResponse>.CreateAsync(
            responseQuery,
            page,
            pageSize,
            cancellationToken);

        var data = pagedList.ToList();
        var path = httpContextAccessor.HttpContext?.Request.Path.Value ?? "/{{route}}";

        var meta = new PaginationMeta
        {
            CurrentPage = pagedList.CurrentPage,
            From = pagedList.TotalCount == 0 ? null : (pagedList.CurrentPage - 1) * pagedList.PageSize + 1,
            LastPage = pagedList.TotalPages,
            Path = path,
            PerPage = pagedList.PageSize,
            To = pagedList.TotalCount == 0 ? null : (pagedList.CurrentPage - 1) * pagedList.PageSize + data.Count,
            Total = pagedList.TotalCount
        };

        string BuildLink(int targetPage)
        {
            var search = Uri.EscapeDataString(request.Search?.Trim() ?? string.Empty);
            return $"{path}?page={targetPage}&per_page={pagedList.PageSize}&search={search}&sort_column={request.SortColumn}&sort_order={request.SortOrder}";
        }

        var links = new PaginationLinks
        {
            First = BuildLink(1),
            Last = BuildLink(Math.Max(pagedList.TotalPages, 1)),
            Prev = pagedList.HasPrevious ? BuildLink(pagedList.CurrentPage - 1) : null,
            Next = pagedList.HasNext ? BuildLink(pagedList.CurrentPage + 1) : null
        };

        return new PaginatedResponse<{{entityName}}ListResponse>(data, meta, links);
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
        app.MapGet("{{route}}", async ([AsParameters] {{featureName}}Request request, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<{{featureName}}Query, PaginatedResponse<{{entityName}}ListResponse>>(
                new {{featureName}}Query(request),
                ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof({{featureName}}Endpoint))
        .WithTags("{{pluralName}}")
        .WithSummary("Get {{pluralName}}")
        .Produces<PaginatedResponse<{{entityName}}ListResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermissions(AppPermission.{{Permission(pluralName, "View")}});
    }
}
"""));
    }

    private static void AddBulkDeleteTemplates(
        ICollection<GeneratedFile> files,
        string rootNamespace,
        string entityName,
        string pluralName,
        string route)
    {
        var featureName = $"BulkDelete{pluralName}";
        var featureNamespace = NamespaceFor(rootNamespace, pluralName, featureName);

        files.Add(new GeneratedFile(
            $"{featureName}/{featureName}Command.cs",
            $$"""
using {{rootNamespace}}.Models.Shared;

namespace {{featureNamespace}};

public sealed record {{featureName}}Command(IReadOnlyCollection<Guid> Ids) : ICommand<ResultResponse>;
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
        RuleFor(x => x.Ids)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(ids => ids.Count <= 100)
            .WithMessage("A maximum of 100 ids can be deleted in one request.");

        RuleForEach(x => x.Ids)
            .NotEmpty()
            .When(x => x.Ids is not null);
    }
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
        if (command.Ids is null || command.Ids.Count == 0)
        {
            throw new ArgumentException("At least one id is required.");
        }

        var ids = command.Ids.Distinct().ToArray();
        var entities = await context.Set<{{entityName}}>()
            .Where(entity => ids.Contains(entity.Id))
            .ToListAsync(cancellationToken);

        if (entities.Count != ids.Length)
        {
            throw new KeyNotFoundException("One or more {{pluralName}} were not found.");
        }

        context.Set<{{entityName}}>().RemoveRange(entities);
        await context.SaveChangesAsync(cancellationToken);

        return new ResultResponse
        {
            Message = $"{entities.Count} {{pluralName}} deleted successfully"
        };
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
        app.MapDelete("{{route}}/bulk", async ([FromBody] {{featureName}}Command command, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendCommandAsync<{{featureName}}Command, ResultResponse>(command, ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof({{featureName}}Endpoint))
        .WithTags("{{pluralName}}")
        .WithSummary("Bulk delete {{pluralName}}")
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
}
