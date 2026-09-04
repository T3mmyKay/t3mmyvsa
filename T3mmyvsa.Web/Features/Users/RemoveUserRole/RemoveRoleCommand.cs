namespace T3mmyvsa.Features.Users.RemoveUserRole;

public record RemoveRoleCommand(string UserId, string RoleName) : ICommand;
