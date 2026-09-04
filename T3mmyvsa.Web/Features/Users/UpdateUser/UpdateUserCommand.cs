namespace T3mmyvsa.Features.Users.UpdateUser;

public record UpdateUserCommand : ICommand
{
    public required Guid UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string PhoneNumber { get; init; }
}
