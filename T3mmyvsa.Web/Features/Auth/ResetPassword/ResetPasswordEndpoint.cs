namespace T3mmyvsa.Features.Auth.ResetPassword;

public class ResetPasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/reset-password", async ([FromBody] ResetPasswordCommand command, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.SendCommandAsync(command, ct);
            return Results.Ok();
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(ResetPasswordEndpoint))
        .WithTags("Auth")
        .WithSummary("Reset password")
        .WithDescription("Resets the user's password using the provided token.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .AllowAnonymous();
    }
}
