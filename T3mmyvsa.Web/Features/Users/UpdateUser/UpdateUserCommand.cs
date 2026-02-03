namespace T3mmyvsa.Features.Users.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string PhoneNumber,
    List<string>? Roles = null
) : ICommand;
