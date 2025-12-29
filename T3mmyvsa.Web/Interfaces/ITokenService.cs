using System.Security.Claims;
using T3mmyvsa.Entities;

namespace T3mmyvsa.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
