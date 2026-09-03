using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.AssignUserRole;

public class AssignRoleCommandHandler(UserManager<User> userManager, IUserRoleService userRoleService)
    : ICommandHandler<AssignRoleCommand>
{
    public async Task Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("User not found.");

        await userRoleService.SetExactRoleAsync(user, request.RoleName, cancellationToken);
    }
}
