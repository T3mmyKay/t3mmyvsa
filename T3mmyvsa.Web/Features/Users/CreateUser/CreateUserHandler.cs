using T3mmyvsa.Entities;

namespace T3mmyvsa.Features.Users.CreateUser;

public class CreateUserHandler(UserManager<User> userManager) : ICommandHandler<CreateUserCommand, string>
{
    public async Task<string> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = new User
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PhoneNumber = command.PhoneNumber,
            EmailConfirmed = true // Auto-confirm for admin created users
        };

        var result = await userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        if (command.Roles.Count != 0)
        {
            var roleResult = await userManager.AddToRolesAsync(user, command.Roles);
            if (!roleResult.Succeeded)
            {
                // Rollback user creation if role assignment fails
                await userManager.DeleteAsync(user);

                var errors = string.Join(" ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Role assignment failed: {errors}");
            }
        }

        return user.Id;
    }
}
