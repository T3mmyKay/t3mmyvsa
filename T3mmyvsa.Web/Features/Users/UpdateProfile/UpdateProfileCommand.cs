namespace T3mmyvsa.Features.Users.UpdateProfile;

public record UpdateProfileCommand(
    [StringLength(100, MinimumLength = 2)] string? FirstName,
    [StringLength(100, MinimumLength = 2)] string? LastName,
    [Phone] string? PhoneNumber
) : ICommand;
