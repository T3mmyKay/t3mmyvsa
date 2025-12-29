using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.ManageRoles;

public class UserRolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("users/{id}/roles").WithTags("Users");

        group.MapGet("", async (string id, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                var response = await mediator.SendQueryAsync<GetUserRolesQuery, List<string>>(new GetUserRolesQuery(id), ct);
                return Results.Ok(response);
            }
            catch (KeyNotFoundException) { return Results.NotFound("User not found."); }
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

        group.MapGet("me", async (IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<GetUserRolesQuery, List<string>>(new GetUserRolesQuery("me"), ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName("GetCurrentUserRoles")
        .WithSummary("Get current user roles")
        .WithDescription("Retrieves all roles assigned to the currently authenticated user.")
        .Produces<List<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();

        group.MapPost("", async (string id, [FromBody] AssignRoleRequest request, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                await mediator.SendCommandAsync<AssignRoleCommand>(new AssignRoleCommand(id, request.RoleName), ct);
                return Results.Ok($"Role '{request.RoleName}' assigned to user.");
            }
            catch (KeyNotFoundException) { return Results.NotFound("User not found."); }
            catch (InvalidOperationException ex) { return Results.BadRequest(ex.Message); }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(AssignRoleCommand))
        .WithSummary("Assign role to user")
        .WithDescription("Assigns a specific role to a user.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.UsersManageRoles);

        group.MapDelete("{roleName}", async (string id, string roleName, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                await mediator.SendCommandAsync<RemoveRoleCommand>(new RemoveRoleCommand(id, roleName), ct);
                return Results.Ok($"Role '{roleName}' removed from user.");
            }
            catch (KeyNotFoundException) { return Results.NotFound("User not found."); }
            catch (InvalidOperationException ex) { return Results.BadRequest(ex.Message); }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(RemoveRoleCommand))
        .WithSummary("Remove role from user")
        .WithDescription("Removes a specific role from a user.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.UsersManageRoles);
    }
}

public record AssignRoleRequest(string RoleName);
