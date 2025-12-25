using System.Security.Claims;
using T3mmyvsa.Entities;

namespace T3mmyvsa.Features.Users.UpdateProfile;

public class UpdateProfileCommandHandler(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
    : ICommandHandler<UpdateProfileCommand>
{
    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        // Only update if provided (even if empty string to clear? Or just if non-null? 
        // The record allows nulls. If null, we skip? Or do we treat null as 'no change'?
        // Usually, in PATCH or specific update commands, null might mean no change.
        // Let's assume common behavior: if it's sent, it's updated. But since C# record properties are nullable reference types, they could be null if not sent in JSON (if optional).

        // Let's update PhoneNumber if provided.
        if (request.PhoneNumber != null)
        {
            await userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
        }

        // For first/last name, we just update property.
        // If request.FirstName is null, should we keep it as is?
        // Let's check typical pattern. Usually, for a full update form, all fields are sent.
        // If it's partial, we might check for null.
        // Let's assume if it's not null, we update.
        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Profile update failed: {errors}");
        }
    }
}
