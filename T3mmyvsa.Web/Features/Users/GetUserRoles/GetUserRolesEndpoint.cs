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
                try
                {
                    var response =
                        await mediator.SendQueryAsync<GetUserRolesQuery, List<string>>(new GetUserRolesQuery(id), ct);
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException)
                {
                    return Results.NotFound("User not found.");
                }
            })
            .HasApiVersion(1)
            .HasApiVersion(2)
            .WithName(nameof(GetUserRolesQuery))
            .WithSummary("Get user roles")
            .WithDescription("Retrieves all roles assigned to a specific user.")
            .Produces<List<string>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .HasPermissions(AppPermission.UsersView);

        // Keep "me" endpoint here as it relates to getting roles for current user
        app.MapGet("users/me/roles", async (IMediator mediator, CancellationToken ct) =>
            {
                var response =
                    await mediator.SendQueryAsync<GetUserRolesQuery, List<string>>(new GetUserRolesQuery("me"), ct);
                return Results.Ok(response);
            })
            .HasApiVersion(1)
            .HasApiVersion(2)
            .WithTags("Users")
            .WithName("GetCurrentUserRoles")
            .WithSummary("Get current user roles")
            .WithDescription("Retrieves all roles assigned to the currently authenticated user.")
            .Produces<List<string>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }
}
