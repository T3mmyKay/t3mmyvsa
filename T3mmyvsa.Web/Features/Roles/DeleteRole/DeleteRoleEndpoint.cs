using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Roles.DeleteRole;

public class DeleteRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("roles/{roleId}", async (string roleId, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                var command = new DeleteRoleCommand(roleId);
                var response = await mediator.SendCommandAsync<DeleteRoleCommand, DeleteRoleResponse>(command, ct);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(DeleteRoleEndpoint))
        .WithTags("Roles")
        .WithSummary("Delete a role")
        .WithDescription("Deletes an application role. Protected system roles (Admin, User) cannot be deleted.")
        .Produces<DeleteRoleResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermissions(AppPermission.RolesDelete);
    }
}
