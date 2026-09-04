namespace T3mmyvsa.Features.Users.GetUserRoles;

public record GetUserRolesQuery(string UserId) : IQuery<List<string>>;
