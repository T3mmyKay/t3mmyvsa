using T3mmyvsa.Entities;

namespace T3mmyvsa.Features.Users.ManageRoles;

public class AssignRoleCommandHandler(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    : ICommandHandler<AssignRoleCommand>
{
    public async Task Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId) ?? throw new KeyNotFoundException("User not found.");
        if (!await roleManager.RoleExistsAsync(request.RoleName))
        {
            throw new InvalidOperationException("Role does not exist.");
        }

        var result = await userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
