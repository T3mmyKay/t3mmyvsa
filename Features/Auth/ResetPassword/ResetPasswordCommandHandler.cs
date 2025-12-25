using T3mmyvsa.Entities;

namespace T3mmyvsa.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandler(UserManager<User> userManager) : ICommandHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // To prevent enumeration attacks, we shouldn't reveal if the user exists
            // But for reset password flow, if the token is invalid, it will fail anyway.
            // If user is null, we can't verify token.
            // We could just return success, or throw a generic "Invalid request" error.
            // For now, let's treat it as a failure but maybe mask it if strictly needed.
            throw new InvalidOperationException("Invalid request");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password reset failed: {errors}");
        }
    }
}
