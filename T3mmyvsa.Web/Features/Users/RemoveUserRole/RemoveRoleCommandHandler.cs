using T3mmyvsa.Entities;
using T3mmyvsa.Exceptions;

namespace T3mmyvsa.Features.Users.RemoveUserRole;

public class RemoveRoleCommandHandler(UserManager<User> userManager) : ICommandHandler<RemoveRoleCommand>
{
    public async Task Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        _ = await userManager.FindByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("User not found.");

        throw new ConflictException(
            "A user must always have exactly one role. Assign a replacement role instead of removing the current role.");
    }
}
