namespace VehicleRegistry.Features.Roles.DeleteRole;

public record DeleteRoleCommand([Required] string RoleId) : ICommand<DeleteRoleResponse>;
