using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.ManagePermissions;

public class UserPermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{id}/permissions", async (string id, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                var response = await mediator.SendQueryAsync<GetUserPermissionsQuery, List<string>>(new GetUserPermissionsQuery(id), ct);
                return Results.Ok(response);
            }
            catch (KeyNotFoundException) { return Results.NotFound("User not found."); }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(GetUserPermissionsQuery))
        .WithTags("Users")
        .WithSummary("Get user permissions")
        .WithDescription("Retrieves all permission claims aggregated from the user's roles.")
        .Produces<List<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.UsersView);

        app.MapGet("users/me/permissions", async (IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<GetUserPermissionsQuery, List<string>>(new GetUserPermissionsQuery("me"), ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName("GetCurrentUserPermissions") // Custom name to avoid collision or ambiguity if reusing Query type name

        .WithTags("Users")
        .WithSummary("Get current user permissions")
        .WithDescription("Retrieves all permission claims aggregated from the currently authenticated user's roles.")
        .Produces<List<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }
}
