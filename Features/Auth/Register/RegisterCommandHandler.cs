using T3mmyvsa.Entities;

namespace T3mmyvsa.Features.Auth.Register;

public class RegisterCommandHandler(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    : ICommandHandler<RegisterCommand>
{
    public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userExists = await userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
        {
            throw new InvalidOperationException("User already exists!");
        }

        var user = new User
        {
            Email = request.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        await userManager.AddToRoleAsync(user, "User");
    }
}
