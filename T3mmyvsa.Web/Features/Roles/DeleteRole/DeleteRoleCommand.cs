namespace T3mmyvsa.Features.Roles.DeleteRole;

public record DeleteRoleCommand([Required] string RoleId) : ICommand<DeleteRoleResponse>;
