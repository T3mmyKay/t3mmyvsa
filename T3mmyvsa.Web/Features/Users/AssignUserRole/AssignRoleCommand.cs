namespace T3mmyvsa.Features.Users.AssignUserRole;

public record AssignRoleCommand(
    [Required] string UserId,
    [Required] string RoleName
) : ICommand;