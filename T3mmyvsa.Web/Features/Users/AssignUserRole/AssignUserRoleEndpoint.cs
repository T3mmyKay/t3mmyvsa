using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.AssignUserRole;

public class AssignUserRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{id}/roles", async (string id, [FromBody] AssignRoleCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var cmd = command with { UserId = id };
            await mediator.SendCommandAsync<AssignRoleCommand>(cmd, ct);
            return Results.Ok($"Role '{command.RoleName}' assigned to user.");
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithTags("Users")
        .WithName(nameof(AssignRoleCommand))
        .WithSummary("Assign role to user")
        .WithDescription("Replaces the user's current role with the specified role.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.UsersManageRoles);
    }
}
