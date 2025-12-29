namespace T3mmyvsa.Features.Users.UpdateProfile;

public class UpdateProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("users/profile", async ([FromBody] UpdateProfileCommand command, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                await mediator.SendCommandAsync(command, ct);
                return Results.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new ProblemDetails { Title = "Update Profile Failed", Detail = ex.Message });
            }
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(UpdateProfileEndpoint))
        .WithTags("Users")
        .WithSummary("Update user profile")
        .WithDescription("Updates the authenticated user's profile information.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }
}
