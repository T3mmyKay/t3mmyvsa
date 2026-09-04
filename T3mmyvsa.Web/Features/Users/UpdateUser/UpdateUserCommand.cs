namespace T3mmyvsa.Features.Users.UpdateUser;

public record UpdateUserCommand : ICommand
{
    public required Guid UserId { get; init; }

    [Required, StringLength(100, MinimumLength = 2)]
    public required string FirstName { get; init; }

    [Required, StringLength(100, MinimumLength = 2)]
    public required string LastName { get; init; }

    [Required, Phone]
    public required string PhoneNumber { get; init; }
}
