namespace T3mmyvsa.Features.Users.CreateUser;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password,
    string Role) : ICommand<string>;
