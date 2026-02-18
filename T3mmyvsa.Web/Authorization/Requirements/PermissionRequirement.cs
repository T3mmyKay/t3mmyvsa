using Microsoft.AspNetCore.Authorization;

namespace T3mmyvsa.Authorization.Requirements;

public class PermissionRequirement(
    string[] requiredPermissions,
    bool requireAll = true) : IAuthorizationRequirement
{
    public string[] RequiredPermissions { get; } = requiredPermissions;
    public bool RequireAll { get; } = requireAll;
}
