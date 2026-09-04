using T3mmyvsa.Entities;
using T3mmyvsa.Interfaces;

namespace T3mmyvsa.Features.Users.UpdateProfile;

public class UpdateProfileCommandHandler(UserManager<User> userManager, ICurrentUserService currentUserService)
    : ICommandHandler<UpdateProfileCommand>
{
    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await userManager.FindByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (request.FirstName is not null)
        {
            user.FirstName = request.FirstName.Trim();
        }

        if (request.LastName is not null)
        {
            user.LastName = request.LastName.Trim();
        }

        if (request.PhoneNumber is not null)
        {
            var phoneResult = await userManager.SetPhoneNumberAsync(user, request.PhoneNumber.Trim());
            if (!phoneResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Profile update failed: {string.Join(", ", phoneResult.Errors.Select(e => e.Description))}");
            }
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Profile update failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
