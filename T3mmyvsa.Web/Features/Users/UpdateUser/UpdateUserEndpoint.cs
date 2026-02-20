using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.UpdateUser;

public class UpdateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("users/{id:guid}", async (Guid id, [FromBody] UpdateUserCommand command, IMediator mediator, CancellationToken ct) =>
        {
            if (id != command.UserId) return Results.BadRequest("Mismatched ID.");
            await mediator.SendCommandAsync(command, ct);
            return Results.NoContent();
        })
        .HasApiVersion(1)
        .WithName(nameof(UpdateUserEndpoint))
        .WithTags("Users")
        .WithSummary("Update a user")
        .WithDescription("Updates a user's profile and roles. Requires admin permissions. Note: 'Roles' expects a list of Role Names (e.g., 'Admin', 'User'), not IDs.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermissions(AppPermission.UsersUpdate);
    }
}


