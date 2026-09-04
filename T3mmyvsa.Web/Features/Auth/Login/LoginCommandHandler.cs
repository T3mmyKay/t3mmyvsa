using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Auth.Login;

public class LoginCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IAuthSessionService authSessionService)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var tokens = await authSessionService.CreateSessionAsync(user, cancellationToken);
        return new LoginResponse(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresAt.UtcDateTime);
    }
}
