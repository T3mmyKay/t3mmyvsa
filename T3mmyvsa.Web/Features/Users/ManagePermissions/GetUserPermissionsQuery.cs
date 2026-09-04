namespace T3mmyvsa.Features.Users.ManagePermissions;

public record GetUserPermissionsQuery(string UserId) : IQuery<List<string>>;
