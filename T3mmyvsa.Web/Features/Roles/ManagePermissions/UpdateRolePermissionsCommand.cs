namespace T3mmyvsa.Features.Roles.ManagePermissions;

public record UpdateRolePermissionsCommand([Required] string RoleId, [Required, MinLength(1)] List<string> Permissions) : ICommand;
