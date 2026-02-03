using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.ReactivateUser;

public class ReactivateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{id:guid}/reactivate", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.SendCommandAsync(new ReactivateUserCommand(id), ct);
            return Results.NoContent();
        })
        .HasApiVersion(1)
        .WithName(nameof(ReactivateUserEndpoint))
        .WithTags("Users")
        .WithSummary("Reactivate a user")
        .WithDescription("Reactivates a previously deactivated user by clearing their lockout status.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermissions(AppPermission.UsersDeactivate);
    }
}
