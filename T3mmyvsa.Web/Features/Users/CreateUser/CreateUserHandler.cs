using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.CreateUser;

public class CreateUserHandler(UserManager<User> userManager, IUserRoleService userRoleService)
    : ICommandHandler<CreateUserCommand, string>
{
    public async Task<string> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(command.Email) is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            UserName = command.Email.Trim(),
            Email = command.Email.Trim(),
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            PhoneNumber = command.PhoneNumber,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"User creation failed: {string.Join(" ", result.Errors.Select(x => x.Description))}");
        }

        try
        {
            await userRoleService.SetExactRoleAsync(user, command.Role, cancellationToken);
        }
        catch
        {
            await userManager.DeleteAsync(user);
            throw;
        }

        return user.Id;
    }
}
