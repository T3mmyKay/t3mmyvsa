namespace T3mmyvsa.Features.Users.ReactivateUser;

public record ReactivateUserCommand(Guid UserId) : ICommand;
