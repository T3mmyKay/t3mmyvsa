namespace T3mmyvsa.Features.Users.ManageRoles;

public record RemoveRoleCommand([Required] string UserId, [Required] string RoleName) : ICommand;
