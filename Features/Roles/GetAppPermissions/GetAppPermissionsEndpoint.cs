namespace T3mmyvsa.Features.Roles.GetAppPermissions;

public class GetAppPermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("permissions");

        group.MapGet("", async (IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<GetAppPermissionsQuery, List<AppPermissionResponse>>(new GetAppPermissionsQuery(), ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(GetAppPermissionsEndpoint))
        .WithTags("Roles")
        .WithSummary("Get all application permissions")
        .WithDescription("Retrieves a list of all available application permissions.")
        .Produces<List<AppPermissionResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization();
    }
}
