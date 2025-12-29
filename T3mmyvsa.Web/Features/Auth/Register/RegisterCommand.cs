namespace T3mmyvsa.Features.Auth.Register;

public record RegisterCommand(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    [Required] string FirstName,
    [Required] string LastName
) : ICommand;
