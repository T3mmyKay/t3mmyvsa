namespace T3mmyvsa.Features.Auth.Login;

public class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async ([FromBody] LoginCommand command, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                var response = await mediator.SendCommandAsync<LoginCommand, LoginResponse>(command, ct);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(LoginEndpoint))
        .WithTags("Auth")
        .WithSummary("User login")
        .WithDescription("Authenticates a user and returns access and refresh tokens.")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}
