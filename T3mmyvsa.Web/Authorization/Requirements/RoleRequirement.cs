using Microsoft.AspNetCore.Authorization;

namespace T3mmyvsa.Authorization.Requirements;

public class RoleRequirement(params string[] allowedRoles) : IAuthorizationRequirement
{
    public string[] AllowedRoles { get; } = allowedRoles;
}
