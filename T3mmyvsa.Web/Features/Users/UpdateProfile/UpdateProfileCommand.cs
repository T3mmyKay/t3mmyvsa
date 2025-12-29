namespace T3mmyvsa.Features.Users.UpdateProfile;

public record UpdateProfileCommand(
    string? FirstName,
    string? LastName,
    [Phone] string? PhoneNumber
) : ICommand;
