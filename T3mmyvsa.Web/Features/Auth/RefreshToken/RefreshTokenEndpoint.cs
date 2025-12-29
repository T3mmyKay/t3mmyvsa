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
            catch (UnauthorizedAccessException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(RefreshTokenEndpoint))
        .WithTags("Auth")
        .WithSummary("Refresh access token")
        .WithDescription("Issues a new access token and refresh token using a valid refresh token.")
        .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}
