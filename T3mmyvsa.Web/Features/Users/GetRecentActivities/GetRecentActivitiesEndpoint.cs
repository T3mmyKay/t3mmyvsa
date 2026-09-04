using T3mmyvsa.Models.Shared;

namespace T3mmyvsa.Features.Users.GetRecentActivities;

public class GetRecentActivitiesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users/activities", async ([AsParameters] GetRecentActivitiesQuery query, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<GetRecentActivitiesQuery, PaginatedResponse<RecentActivityResponse>>(query, ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName(nameof(GetRecentActivitiesEndpoint))
        .WithTags("Users")
        .WithSummary("Get recent activities")
        .WithDescription("Retrieves paginated audit activity for the current user or another user when the caller has Users.ViewActivity.")
        .Produces<PaginatedResponse<RecentActivityResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization();
    }
}
