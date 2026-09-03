namespace T3mmyvsa.Features.Auth.RefreshToken;

public class RefreshTokenEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh-token", async ([FromBody] RefreshTokenCommand command, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                var response = await mediator.SendCommandAsync<RefreshTokenCommand, RefreshTokenResponse>(command, ct);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(RefreshTokenEndpoint))
        .WithTags("Auth")
        .WithSummary("Refresh access token")
        .WithDescription("Rotates a valid refresh credential and issues a replacement token pair. The accessToken field is accepted for backwards compatibility but is not trusted for refresh authorization.")
        .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}
