using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler(UserManager<User> userManager, ITokenService tokenService)
    : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken) ?? throw new UnauthorizedAccessException("Invalid access token or refresh token");
        var username = principal.Identity?.Name;

        var user = await userManager.FindByNameAsync(username!);

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid access token or refresh token");
        }

        var newAccessToken = await tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return new RefreshTokenResponse(newAccessToken, newRefreshToken, DateTime.UtcNow.AddDays(7));
    }
}
