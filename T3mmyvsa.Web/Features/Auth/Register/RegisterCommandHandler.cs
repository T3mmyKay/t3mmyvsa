using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Auth.Register;

public class RegisterCommandHandler(UserManager<User> userManager, IUserRoleService userRoleService)
    : ICommandHandler<RegisterCommand>
{
    public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            throw new InvalidOperationException("User already exists!");
        }

        var user = new User
        {
            Email = email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true,
            LockoutEnabled = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        try
        {
            await userRoleService.SetExactRoleAsync(user, AppRole.User.ToString(), cancellationToken);
        }
        catch
        {
            await userManager.DeleteAsync(user);
            throw;
        }
    }
}
