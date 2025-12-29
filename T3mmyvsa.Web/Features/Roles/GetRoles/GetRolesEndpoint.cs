using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Roles.GetRoles;

public class GetRolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("roles");

        group.MapGet("", async (IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<GetRolesQuery, List<RoleResponse>>(new GetRolesQuery(), ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(GetRolesEndpoint))
        .WithTags("Roles")
        .WithSummary("Get all roles")
        .WithDescription("Retrieves a list of all defined application roles.")
        .Produces<List<RoleResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermissions(AppPermission.RolesView);
    }
}
