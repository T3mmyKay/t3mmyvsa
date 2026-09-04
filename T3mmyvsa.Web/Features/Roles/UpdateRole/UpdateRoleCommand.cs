namespace T3mmyvsa.Features.Roles.UpdateRole;

public record UpdateRoleCommand(string Id, string RoleName) : ICommand<UpdateRoleResponse>;
