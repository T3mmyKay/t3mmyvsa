using T3mmyvsa.Entities;

namespace T3mmyvsa.Features.Users.ReactivateUser;

public class ReactivateUserHandler(UserManager<User> userManager) : ICommandHandler<ReactivateUserCommand>
{
    public async Task Handle(ReactivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString())
            ?? throw new KeyNotFoundException($"User with ID {command.UserId} not found.");

        if (user.IsActive)
        {
            return;
        }

        user.IsActive = true;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Description)));
        }
    }
}
