using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;
using T3mmyvsa.Features.Users.GetUser;
using T3mmyvsa.Features.Users.GetUsers;

namespace T3mmyvsa.Features.Users.GetUser;

public class GetUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{id}", async (string id, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<GetUserQuery, UserResponse?>(new GetUserQuery(id), ct);

            if (response is null)
            {
                return Results.NotFound("User not found.");
            }

            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(GetUserEndpoint))
        .WithTags("Users")
        .WithSummary("Get a user by ID")
        .WithDescription("Retrieves details of a specific user by their unique identifier.")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermissions(AppPermission.UsersView);
    }
}
