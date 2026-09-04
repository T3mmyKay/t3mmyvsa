namespace T3mmyvsa.Features.Roles.CreateRole;

public record CreateRoleCommand(string RoleName) : ICommand<CreateRoleResponse>;
