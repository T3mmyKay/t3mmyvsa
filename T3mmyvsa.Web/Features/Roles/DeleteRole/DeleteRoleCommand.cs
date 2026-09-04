namespace T3mmyvsa.Features.Roles.DeleteRole;

public record DeleteRoleCommand(string RoleId) : ICommand<DeleteRoleResponse>;
