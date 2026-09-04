using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.GetUserRoles;

public class GetUserRolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("users/{id}/roles").WithTags("Users");

        group.MapGet("", async (string id, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<GetUserRolesQuery, List<string>>(new GetUserRolesQuery(id), ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(GetUserRolesQuery))
        .WithSummary("Get user roles")
        .WithDescription("Retrieves the role assigned to a specific user. The collection shape is retained for compatibility.")
        .Produces<List<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.UsersView);

        app.MapGet("users/me/roles", async (IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<GetUserRolesQuery, List<string>>(new GetUserRolesQuery("me"), ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithTags("Users")
        .WithName("GetCurrentUserRoles")
        .WithSummary("Get current user roles")
        .WithDescription("Retrieves the role assigned to the currently authenticated user. The collection shape is retained for compatibility.")
        .Produces<List<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }
}
