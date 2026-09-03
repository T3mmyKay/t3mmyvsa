using T3mmyvsa.Entities;

namespace T3mmyvsa.Features.Users.UpdateUser;

public class UpdateUserHandler(UserManager<User> userManager) : ICommandHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString())
            ?? throw new KeyNotFoundException($"User with ID {command.UserId} not found.");

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.PhoneNumber = command.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"User update failed: {string.Join(", ", result.Errors.Select(x => x.Description))}");
        }
    }
}
