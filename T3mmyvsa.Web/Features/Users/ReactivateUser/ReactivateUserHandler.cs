using T3mmyvsa.Entities;

namespace T3mmyvsa.Features.Users.ReactivateUser;

public class ReactivateUserHandler(UserManager<User> userManager) : ICommandHandler<ReactivateUserCommand>
{
    public async Task Handle(ReactivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString()) ?? throw new KeyNotFoundException($"User with ID {command.UserId} not found.");
        user.LockoutEnd = null;
        await userManager.UpdateAsync(user);
    }
}
