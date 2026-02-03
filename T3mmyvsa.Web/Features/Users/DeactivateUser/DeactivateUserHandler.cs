using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Entities;

namespace T3mmyvsa.Features.Users.DeactivateUser;

public class DeactivateUserHandler(UserManager<User> userManager) : ICommandHandler<DeactivateUserCommand>
{
    public async Task Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString()) ?? throw new KeyNotFoundException($"User with ID {command.UserId} not found.");

        // Prevent deactivating Admin users
        var roles = await userManager.GetRolesAsync(user);
        if (roles.Contains(AppRole.Admin.ToString()))
        {
            throw new InvalidOperationException("Admin users cannot be deactivated.");
        }

        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await userManager.UpdateAsync(user);
    }
}
