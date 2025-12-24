namespace T3mmyvsa.Features.Roles.ManagePermissions;

public record GetRolePermissionsQuery([Required] string RoleId) : IQuery<List<string>>;
