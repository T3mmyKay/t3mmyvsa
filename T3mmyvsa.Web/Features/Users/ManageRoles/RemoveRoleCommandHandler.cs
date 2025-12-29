using T3mmyvsa.Entities;

namespace T3mmyvsa.Features.Users.ManageRoles;

public class RemoveRoleCommandHandler(UserManager<User> userManager) : ICommandHandler<RemoveRoleCommand>
{
    public async Task Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId) ?? throw new KeyNotFoundException("User not found.");
        var result = await userManager.RemoveFromRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
