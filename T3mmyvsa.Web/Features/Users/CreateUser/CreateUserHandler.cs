using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.CreateUser;

public class CreateUserHandler(UserManager<User> userManager, IUserRoleService userRoleService)
    : ICommandHandler<CreateUserCommand, string>
{
    public async Task<string> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            PhoneNumber = command.PhoneNumber.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            LockoutEnabled = true
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
