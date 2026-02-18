using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using T3mmyvsa.Authorization.Requirements;

namespace T3mmyvsa.Authorization.Providers;

public class CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) 
    : DefaultAuthorizationPolicyProvider(options)
{
    private const string RolePrefix = "HasRole:";
    private const string PermissionPrefix = "HasPermission:";
    private const string HasAnyPermissionPrefix = "HasAnyPermission:";
    private const string HasAnyRolePrefix = "HasAnyRole:";

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check for role-based policy
        if (policyName.StartsWith(RolePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var roles = policyName[RolePrefix.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new RoleRequirement(roles))
                .Build();
            
            return policy;
        }

        // Check for role-based policy (Any)
        if (policyName.StartsWith(HasAnyRolePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var roles = policyName[HasAnyRolePrefix.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new RoleRequirement(roles))
                .Build();

            return policy;
        }

        // Check for permission-based policy (All)
        if (policyName.StartsWith(PermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissions = policyName[PermissionPrefix.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permissions))
                .Build();
            
            return policy;
        }

        // Check for permission-based policy (Any)
        if (policyName.StartsWith(HasAnyPermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissions = policyName[HasAnyPermissionPrefix.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permissions, requireAll: false))
                .Build();

            return policy;
        }

        // Fall back to the default policy provider
        return await base.GetPolicyAsync(policyName);
    }
}
