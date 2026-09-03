using System.Security.Claims;
using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Auth.ChangePassword;

public class ChangePasswordCommandHandler(
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor,
    IAuthSessionService authSessionService)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await userManager.FindByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found.");

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Password change failed: {string.Join(", ", result.Errors.Select(x => x.Description))}");
        }

        await authSessionService.RevokeAllSessionsAsync(user.Id, cancellationToken);
    }
}
