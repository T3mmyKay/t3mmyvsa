using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Roles.UpdateRole;

public class UpdateRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("roles/{id:guid}", async (string id, [FromBody] UpdateRoleCommand command, IMediator mediator, CancellationToken ct) =>
        {
            if (id != command.Id) return Results.BadRequest("Mismatched ID.");

            try
            {
                var response = await mediator.SendCommandAsync<UpdateRoleCommand, UpdateRoleResponse>(command, ct);
                return Results.Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(UpdateRoleEndpoint))
        .WithTags("Roles")
        .WithSummary("Update a role")
        .WithDescription("Updates the name of an existing role.")
        .Produces<UpdateRoleResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.RolesUpdate);
    }
}
