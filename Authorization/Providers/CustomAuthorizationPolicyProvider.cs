using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using T3mmyvsa.Authorization.Requirements;

namespace T3mmyvsa.Authorization.Providers;

public class CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) 
    : DefaultAuthorizationPolicyProvider(options)
{
    private const string RolePrefix = "HasRole:";
    private const string PermissionPrefix = "HasPermission:";

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

        // Check for permission-based policy
        if (policyName.StartsWith(PermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissions = policyName[PermissionPrefix.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permissions))
                .Build();
            
            return policy;
        }

        // Fall back to the default policy provider
        return await base.GetPolicyAsync(policyName);
    }
}
