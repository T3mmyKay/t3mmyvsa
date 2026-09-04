namespace T3mmyvsa.Features.Roles.ManagePermissions;

public record GetRolePermissionsQuery(string RoleId) : IQuery<List<string>>;
