using System.Security.Claims;
using T3mmyvsa.Authorization;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Auth.Logout;

public sealed class LogoutEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (HttpContext httpContext, IAuthSessionService authSessionService, CancellationToken ct) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionValue = httpContext.User.FindFirstValue(AuthClaimTypes.SessionId);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(sessionValue, out var sessionId))
            {
                return Results.Unauthorized();
            }

            await authSessionService.RevokeSessionAsync(userId, sessionId, ct);
            return Results.NoContent();
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName("LogoutCurrentSession")
        .WithTags("Auth")
        .WithSummary("Logout current session")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();

        app.MapPost("auth/logout-all", async (HttpContext httpContext, IAuthSessionService authSessionService, CancellationToken ct) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            await authSessionService.RevokeAllSessionsAsync(userId, ct);
            return Results.NoContent();
        })
        .HasApiVersion(1)
        .HasApiVersion(2)
        .WithName("LogoutAllSessions")
        .WithTags("Auth")
        .WithSummary("Logout all sessions")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }
}
