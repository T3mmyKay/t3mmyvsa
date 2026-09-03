using T3mmyvsa.Entities;

namespace T3mmyvsa.Interfaces;

public sealed record AuthTokenPair(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    Guid SessionId);

public interface IAuthSessionService
{
    Task<AuthTokenPair> CreateSessionAsync(User user, CancellationToken cancellationToken = default);
    Task<AuthTokenPair> RotateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> IsSessionActiveAsync(string userId, Guid sessionId, CancellationToken cancellationToken = default);
    Task RevokeSessionAsync(string userId, Guid sessionId, CancellationToken cancellationToken = default);
    Task RevokeAllSessionsAsync(string userId, CancellationToken cancellationToken = default);
}
