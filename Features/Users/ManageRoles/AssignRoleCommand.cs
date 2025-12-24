namespace T3mmyvsa.Features.Users.ManageRoles;

public record AssignRoleCommand(
    [Required] string UserId,
    [Required] string RoleName
) : ICommand;
