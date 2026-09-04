using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.DeactivateUser;

public class DeactivateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{id:guid}/deactivate", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.SendCommandAsync(new DeactivateUserCommand(id), ct);
            return Results.NoContent();
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(DeactivateUserEndpoint))
        .WithTags("Users")
        .WithSummary("Deactivate a user")
        .WithDescription("Administratively deactivates a user and revokes all active sessions. Identity lockout remains reserved for authentication security.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .HasPermissions(AppPermission.UsersDeactivate);
    }
}
