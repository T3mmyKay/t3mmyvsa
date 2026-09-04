namespace T3mmyvsa.Features.Auth.Register;

public record RegisterCommand(
    [Required, EmailAddress] string Email,
    [Required, StringLength(100, MinimumLength = 10)] string Password,
    [Required, StringLength(100, MinimumLength = 2)] string FirstName,
    [Required, StringLength(100, MinimumLength = 2)] string LastName
) : ICommand;
