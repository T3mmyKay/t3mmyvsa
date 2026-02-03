using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.AssignUserRole;

public class AssignUserRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{id}/roles",
                async (string id, [FromBody] AssignRoleCommand command, IMediator mediator, CancellationToken ct) =>
                {
                    try
                    {
                        // Override UserId from route
                        var cmd = command with { UserId = id };
                        await mediator.SendCommandAsync<AssignRoleCommand>(cmd, ct);
                        return Results.Ok($"Role '{command.RoleName}' assigned to user.");
                    }
                    catch (KeyNotFoundException)
                    {
                        return Results.NotFound("User not found.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(ex.Message);
                    }
                })
            .HasApiVersion(1)
            .HasApiVersion(2)
            .WithTags("Users")
            .WithName(nameof(AssignRoleCommand))
            .WithSummary("Assign role to user")
            .WithDescription("Assigns a specific role to a user.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .HasPermissions(AppPermission.UsersManageRoles);
    }
}
