using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.DeactivateUser;

public class DeactivateUserHandler(UserManager<User> userManager, IAuthSessionService authSessionService)
    : ICommandHandler<DeactivateUserCommand>
{
    public async Task Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString())
            ?? throw new KeyNotFoundException($"User with ID {command.UserId} not found.");

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Contains(AppRole.Admin.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Admin users cannot be deactivated.");
        }

        if (!user.IsActive)
        {
            return;
        }

        user.IsActive = false;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        await authSessionService.RevokeAllSessionsAsync(user.Id, cancellationToken);
    }
}
