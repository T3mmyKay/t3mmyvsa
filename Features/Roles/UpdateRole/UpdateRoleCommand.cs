namespace T3mmyvsa.Features.Roles.UpdateRole;

public record UpdateRoleCommand([Required] string Id, [Required, StringLength(100, MinimumLength = 2)] string RoleName) : ICommand<UpdateRoleResponse>;
