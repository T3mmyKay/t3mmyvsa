using Microsoft.AspNetCore.RateLimiting;
using T3mmyvsa.Configuration;

namespace T3mmyvsa.Features.Auth.Register;

public class RegisterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/register", async ([FromBody] RegisterCommand command, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.SendCommandAsync<RegisterCommand>(command, ct);
            return Results.Ok(new { Message = "User created successfully!" });
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
        .ProducesProblem(StatusCodes.Status429TooManyRequests)
        .RequireRateLimiting(RateLimitPolicyNames.Registration)
        .AllowAnonymous();
    }
}
