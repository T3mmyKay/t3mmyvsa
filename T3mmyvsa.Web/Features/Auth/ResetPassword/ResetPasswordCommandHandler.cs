using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandler(UserManager<User> userManager, IAuthSessionService authSessionService)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new InvalidOperationException("Invalid request");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Password reset failed: {string.Join(", ", result.Errors.Select(x => x.Description))}");
        }

        await authSessionService.RevokeAllSessionsAsync(user.Id, cancellationToken);
    }
}
