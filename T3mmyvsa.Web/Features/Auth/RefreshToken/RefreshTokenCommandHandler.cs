using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler(IAuthSessionService authSessionService)
    : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokens = await authSessionService.RotateRefreshTokenAsync(request.RefreshToken, cancellationToken);
        return new RefreshTokenResponse(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresAt.UtcDateTime);
    }
}
