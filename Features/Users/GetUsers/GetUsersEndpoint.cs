using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;
using T3mmyvsa.Models.Shared;

namespace T3mmyvsa.Features.Users.GetUsers;

public class GetUsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users", async ([AsParameters] GetUsersRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<GetUsersQuery, PaginatedResponse<UserResponse>>(new GetUsersQuery(request), ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(GetUsersEndpoint))
        .WithTags("Users")
        .WithSummary("Get all users")
        .WithDescription("Retrieves a paginated list of all users.")
        .Produces<PaginatedResponse<UserResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .HasPermissions(AppPermission.UsersView);
    }
}
