namespace T3mmyvsa.Features.Users.CreateUser;

public record CreateUserCommand(
    [Required, StringLength(100, MinimumLength = 2)] string FirstName,
    [Required, StringLength(100, MinimumLength = 2)] string LastName,
    [Required, EmailAddress] string Email,
    [Required, Phone] string PhoneNumber,
    [Required, StringLength(100, MinimumLength = 6)] string Password,
    [Required] List<string> Roles
) : ICommand<string>;
