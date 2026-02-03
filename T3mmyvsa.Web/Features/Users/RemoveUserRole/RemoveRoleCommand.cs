namespace T3mmyvsa.Features.Users.RemoveUserRole;

public record RemoveRoleCommand([Required] string UserId, [Required] string RoleName) : ICommand;