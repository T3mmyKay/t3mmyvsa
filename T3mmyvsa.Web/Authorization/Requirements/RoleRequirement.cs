using Microsoft.AspNetCore.Authorization;

namespace T3mmyvsa.Authorization.Requirements;

public class RoleRequirement(
    string[] allowedRoles,
    bool requireAll = false) : IAuthorizationRequirement
{
    public string[] AllowedRoles { get; } = allowedRoles;
    public bool RequireAll { get; } = requireAll;
}
