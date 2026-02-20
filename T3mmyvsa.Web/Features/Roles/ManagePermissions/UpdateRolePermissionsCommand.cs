namespace T3mmyvsa.Features.Roles.ManagePermissions;

public record UpdateRolePermissionsCommand : ICommand
{
    public required string RoleId { get; init; }
    [Required, MinLength(1)]
    public required List<string> Permissions { get; init; }
}

