using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.CreateUser;

public class CreateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users", async ([FromBody] CreateUserCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.SendCommandAsync<CreateUserCommand, string>(command, ct);
            return Results.Created($"/api/users/{result}", result);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(CreateUserEndpoint))
        .WithTags("Users")
        .WithSummary("Create a new user")
        .WithDescription("Creates a user with exactly one application role. Both user creation and role-management permissions are required.")
        .Produces<string>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .HasPermissions(AppPermission.UsersCreate, AppPermission.UsersManageRoles);
    }
}
