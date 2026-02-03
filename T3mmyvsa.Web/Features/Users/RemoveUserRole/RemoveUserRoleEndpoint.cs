using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.RemoveUserRole;

public class RemoveUserRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/{id}/roles/{roleName}",
                async (string id, string roleName, IMediator mediator, CancellationToken ct) =>
                {
                    try
                    {
                        await mediator.SendCommandAsync<RemoveRoleCommand>(new RemoveRoleCommand(id, roleName), ct);
                        return Results.Ok($"Role '{roleName}' removed from user.");
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
