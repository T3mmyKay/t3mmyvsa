using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using T3mmyvsa.Attributes;
using T3mmyvsa.Configuration;
using T3mmyvsa.Data;
using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Services;

[ScopedService]
public sealed class AuthSessionService(
    AppDbContext db,
    UserManager<User> userManager,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings,
    IHttpContextAccessor httpContextAccessor) : IAuthSessionService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<AuthTokenPair> CreateSessionAsync(User user, CancellationToken cancellationToken = default)
    {
        if (await userManager.IsLockedOutAsync(user))
        {
            throw new UnauthorizedAccessException("This account is not active.");
        }

        var now = DateTimeOffset.UtcNow;
        var refreshToken = GenerateRefreshToken();
        var session = CreateSession(user.Id, refreshToken, now);

        db.AuthSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        var (accessToken, accessExpiresAt) = await tokenService.GenerateAccessTokenAsync(user, session.Id);
        return new AuthTokenPair(accessToken, refreshToken, accessExpiresAt, session.ExpiresAt, session.Id);
    }

    public async Task<AuthTokenPair> RotateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var now = DateTimeOffset.UtcNow;
        var hash = HashRefreshToken(refreshToken);
        var current = await db.AuthSessions.SingleOrDefaultAsync(x => x.RefreshTokenHash == hash, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (current.RevokedAt is not null)
        {
            // Reusing a rotated/revoked refresh token is treated as credential theft.
            await RevokeAllSessionsInternalAsync(current.UserId, now, cancellationToken);
            throw new UnauthorizedAccessException("Refresh token reuse detected; all sessions were revoked.");
        }

        if (current.ExpiresAt <= now)
        {
            current.RevokedAt = now;
            await db.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Refresh token has expired.");
        }

        var user = await userManager.FindByIdAsync(current.UserId)
            ?? throw new UnauthorizedAccessException("User no longer exists.");
        if (await userManager.IsLockedOutAsync(user))
        {
            await RevokeAllSessionsInternalAsync(user.Id, now, cancellationToken);
            throw new UnauthorizedAccessException("This account is not active.");
        }

        var newRefreshToken = GenerateRefreshToken();
        var replacement = CreateSession(user.Id, newRefreshToken, now);

        current.LastUsedAt = now;
        current.RevokedAt = now;
        current.ReplacedBySessionId = replacement.Id;
        db.AuthSessions.Add(replacement);
        await db.SaveChangesAsync(cancellationToken);

        var (accessToken, accessExpiresAt) = await tokenService.GenerateAccessTokenAsync(user, replacement.Id);
        return new AuthTokenPair(accessToken, newRefreshToken, accessExpiresAt, replacement.ExpiresAt, replacement.Id);
    }

    public async Task<bool> IsSessionActiveAsync(string userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || sessionId == Guid.Empty)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var active = await db.AuthSessions.AsNoTracking().AnyAsync(
            x => x.Id == sessionId && x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now,
            cancellationToken);
        if (!active)
        {
            return false;
        }

        var user = await userManager.FindByIdAsync(userId);
        return user is not null && !await userManager.IsLockedOutAsync(user);
    }

    public async Task RevokeSessionAsync(string userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await db.AuthSessions.SingleOrDefaultAsync(
            x => x.Id == sessionId && x.UserId == userId,
            cancellationToken);
        if (session is null || session.RevokedAt is not null)
        {
            return;
        }

        session.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task RevokeAllSessionsAsync(string userId, CancellationToken cancellationToken = default) =>
        RevokeAllSessionsInternalAsync(userId, DateTimeOffset.UtcNow, cancellationToken);

    private async Task RevokeAllSessionsInternalAsync(string userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var sessions = await db.AuthSessions
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.RevokedAt = now;
        }

        if (sessions.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private AuthSession CreateSession(string userId, string refreshToken, DateTimeOffset now)
    {
        var http = httpContextAccessor.HttpContext;
        return new AuthSession
        {
            UserId = userId,
            RefreshTokenHash = HashRefreshToken(refreshToken),
            CreatedAt = now,
            ExpiresAt = now.AddDays(_jwtSettings.RefreshTokenValidityInDays),
            IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = http?.Request.Headers.UserAgent.ToString()
        };
    }

    private static string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string HashRefreshToken(string refreshToken) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
