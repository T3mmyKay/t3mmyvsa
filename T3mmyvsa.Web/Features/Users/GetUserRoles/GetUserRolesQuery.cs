namespace T3mmyvsa.Features.Users.GetUserRoles;

public record GetUserRolesQuery([Required] string UserId) : IQuery<List<string>>;