namespace T3mmyvsa.Features.Auth.ChangePassword;

public class ChangePasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/change-password", async ([FromBody] ChangePasswordCommand command, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.SendCommandAsync(command, ct);
            return Results.Ok();
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(ChangePasswordEndpoint))
        .WithTags("Auth")
        .WithSummary("Change password")
        .WithDescription("Allows an authenticated user to change their password.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }
}
