using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Roles.CreateRole;

public class CreateRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("roles", async ([FromBody] CreateRoleCommand command, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                var response = await mediator.SendCommandAsync<CreateRoleCommand, CreateRoleResponse>(command, ct);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(CreateRoleEndpoint))
        .WithTags("Roles")
        .WithSummary("Create a new role")
        .WithDescription("Creates a new application role.")
        .Produces<CreateRoleResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermissions(AppPermission.RolesCreate);
    }
}
