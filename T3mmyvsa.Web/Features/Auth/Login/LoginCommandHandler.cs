using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Auth.Login;

public class LoginCommandHandler(UserManager<User> userManager, IAuthSessionService authSessionService)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || await userManager.IsLockedOutAsync(user) || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var tokens = await authSessionService.CreateSessionAsync(user, cancellationToken);
        return new LoginResponse(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresAt.UtcDateTime);
    }
}
