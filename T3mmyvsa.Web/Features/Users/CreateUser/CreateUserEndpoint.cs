using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;


namespace T3mmyvsa.Features.Users.CreateUser;

public class CreateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users", async (CreateUserCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.SendCommandAsync<CreateUserCommand, string>(command, ct);
            return Results.Created($"/api/users/{result}", result);
        })
        .HasApiVersion(1)
        .WithName(nameof(CreateUserEndpoint))
        .WithTags("Users")
        .WithSummary("Create a new user")
        .WithDescription("Creates a new user with the specified roles.")
        .Produces<string>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermissions(AppPermission.UsersCreate);
    }
}
