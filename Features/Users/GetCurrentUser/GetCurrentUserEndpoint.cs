namespace T3mmyvsa.Features.Users.GetCurrentUser;

public class GetCurrentUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me", async (IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                var response = await mediator.SendQueryAsync<GetCurrentUserQuery, CurrentUserResponse>(new GetCurrentUserQuery(), ct);
                return Results.Ok(response);
            }
            catch (InvalidOperationException)
            {
                return Results.Unauthorized();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(GetCurrentUserEndpoint))
        .WithTags("Users")
        .WithSummary("Get current user details")
        .WithDescription("Retrieves details, roles, and permissions of the currently authenticated user.")
        .Produces<CurrentUserResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();
    }
}