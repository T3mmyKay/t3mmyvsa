namespace T3mmyvsa.Features.Auth.ForgotPassword;

public class ForgotPasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/forgot-password", async ([FromBody] ForgotPasswordCommand command, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.SendCommandAsync(command, ct);
            return Results.Ok();
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(ForgotPasswordEndpoint))
        .WithTags("Auth")
        .WithSummary("Request password reset")
        .WithDescription("Sends a password reset link to the user's email if the account exists.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .AllowAnonymous();
    }
}
