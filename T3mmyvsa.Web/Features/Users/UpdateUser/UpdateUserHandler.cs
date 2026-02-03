using T3mmyvsa.Entities;

namespace T3mmyvsa.Features.Users.UpdateUser;

public class UpdateUserHandler(UserManager<User> userManager, RoleManager<IdentityRole> roleManager) : ICommandHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString()) ?? throw new KeyNotFoundException($"User with ID {command.UserId} not found.");
        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.PhoneNumber = command.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User update failed: {errors}");
        }

        if (command.Roles != null)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            var rolesToAdd = command.Roles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(command.Roles).ToList();

            if (rolesToAdd.Count != 0)
            {
                // Validate roles exist
                foreach (var role in rolesToAdd)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        throw new InvalidOperationException($"Role '{role}' does not exist.");
                }

                var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to add roles: {errors}");
                }
            }

            if (rolesToRemove.Count != 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to remove roles: {errors}");
                }
            }
        }
    }
}
