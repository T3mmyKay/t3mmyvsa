namespace T3mmyvsa.Features.Roles.CreateRole;

public record CreateRoleCommand([Required, StringLength(100, MinimumLength = 2)] string RoleName) : ICommand<CreateRoleResponse>;
