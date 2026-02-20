using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Roles.ManagePermissions;

public class RolePermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("roles/{id}/permissions").WithTags("Roles");

        group.MapGet("", async (string id, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                var response = await mediator.SendQueryAsync<GetRolePermissionsQuery, List<string>>(new GetRolePermissionsQuery(id), ct);
                return Results.Ok(response);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(GetRolePermissionsQuery))
        .WithSummary("Get role permissions")
        .WithDescription("Retrieves all permission claims associated with a specific role.")
        .Produces<List<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.RolesView);

        group.MapPut("", async (string id, [FromBody] UpdateRolePermissionsCommand command, IMediator mediator, CancellationToken ct) =>
        {
            if (id != command.RoleId) return Results.BadRequest("Mismatched ID.");

            try
            {
                await mediator.SendCommandAsync<UpdateRolePermissionsCommand>(command, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(UpdateRolePermissionsCommand))
        .WithSummary("Update role permissions")
        .WithDescription("Replaces the existing permissions for a role with the provided list.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.RolesUpdate);
    }
}
