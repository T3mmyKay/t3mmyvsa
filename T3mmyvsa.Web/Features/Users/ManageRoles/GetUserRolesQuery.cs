namespace T3mmyvsa.Features.Users.ManageRoles;

public record GetUserRolesQuery([Required] string UserId) : IQuery<List<string>>;
