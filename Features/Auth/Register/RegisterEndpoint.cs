namespace T3mmyvsa.Features.Auth.Register;

public class RegisterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/register", async ([FromBody] RegisterCommand command, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                await mediator.SendCommandAsync<RegisterCommand>(command, ct);
                return Results.Ok(new { Message = "User created successfully!" });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(RegisterEndpoint))
        .WithTags("Auth")
        .WithSummary("User registration")
        .WithDescription("Registers a new user with the default 'User' role.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .AllowAnonymous();
    }
}
