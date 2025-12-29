using Carter;

using Microsoft.AspNetCore.Mvc;
using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Users.GetRecentActivities;

public class GetRecentActivitiesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users/activities", async ([FromQuery] string? userId, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.SendQueryAsync<GetRecentActivitiesQuery, List<RecentActivityResponse>>(new GetRecentActivitiesQuery(userId), ct);
            return Results.Ok(response);
        })
        .HasApiVersion(1)
        .WithName(nameof(GetRecentActivitiesEndpoint))
        .WithTags("Users")
        .WithSummary("Get recent activities")
        .WithDescription("Retrieves recent audit logs for the current user or a specified user (requires permission).")
        .Produces<List<RecentActivityResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        // We require basic auth/permission to hit the endpoint at all.
        // If asking for 'self', standard Users.View (or just Auth) might be enough.
        // If asking for 'others', logic is in Handler, but we need meaningful endpoint permissions.
        // Let's enforce authentication at least.
        .RequireAuthorization();
    }
}
