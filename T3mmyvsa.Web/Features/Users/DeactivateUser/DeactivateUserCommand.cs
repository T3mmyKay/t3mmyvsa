namespace T3mmyvsa.Features.Users.DeactivateUser;

public record DeactivateUserCommand(Guid UserId) : ICommand;
