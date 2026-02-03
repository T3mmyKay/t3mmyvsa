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
        .WithName(nameof(DeactivateUserEndpoint))
        .WithTags("Users")
        .WithSummary("Deactivate a user")
        .WithDescription("Deactivates a user by setting their lockout end date to max value. Admin users cannot be deactivated.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermissions(AppPermission.UsersDeactivate);
    }
}
