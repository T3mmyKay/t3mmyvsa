namespace T3mmyvsa.Features.Users.AssignUserRole;

public record AssignRoleCommand(string UserId, string RoleName) : ICommand;
