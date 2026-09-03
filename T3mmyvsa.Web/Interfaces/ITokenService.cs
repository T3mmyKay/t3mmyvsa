using T3mmyvsa.Entities;

namespace T3mmyvsa.Interfaces;

public interface ITokenService
{
    Task<(string Token, DateTimeOffset ExpiresAt)> GenerateAccessTokenAsync(User user, Guid sessionId);
}
