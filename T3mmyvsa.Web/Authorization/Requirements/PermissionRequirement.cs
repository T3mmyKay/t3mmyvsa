using Microsoft.AspNetCore.Authorization;

namespace T3mmyvsa.Authorization.Requirements;

public class PermissionRequirement(params string[] requiredPermissions) : IAuthorizationRequirement
{
    public string[] RequiredPermissions { get; } = requiredPermissions;
}
