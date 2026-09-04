using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.ManagePermissions;

public class UserPermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{id}/permissions", async (string id, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<GetUserPermissionsQuery, List<string>>(new GetUserPermissionsQuery(id), ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(GetUserPermissionsQuery))
        .WithTags("Users")
        .WithSummary("Get user permissions")
        .WithDescription("Retrieves the user's effective server-side permissions.")
        .Produces<List<string>>(StatusCodes.Status200OK)
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
        .WithName("GetCurrentUserPermissions")
        .WithTags("Users")
        .WithSummary("Get current user permissions")
        .WithDescription("Retrieves the current user's effective server-side permissions.")
        .Produces<List<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }
}
