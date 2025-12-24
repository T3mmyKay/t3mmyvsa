using T3mmyvsa.Authorization.Enums;

namespace T3mmyvsa.Extensions;

public static class AuthorizationExtensions
{
    /// <param name="builder">The route handler builder.</param>
    extension(RouteHandlerBuilder builder)
    {
        /// <summary>
        /// Requires the user to have at least one of the specified roles.
        /// </summary>
        /// <param name="roles">The allowed roles (user must have at least one).</param>
        /// <returns>The route handler builder for chaining.</returns>
        public RouteHandlerBuilder HasRole(params string[] roles)
        {
            ArgumentNullException.ThrowIfNull(roles);

            if (roles.Length == 0)
                throw new ArgumentException("At least one role must be specified.", nameof(roles));

            string policyName = $"HasRole:{string.Join(",", roles)}";
            return builder.RequireAuthorization(policyName);
        }

        /// <summary>
        /// Requires the user to have at least one of the specified roles.
        /// </summary>
        public RouteHandlerBuilder HasRole(params AppRole[] roles)
        {
            return builder.HasRole(roles.Select(r => r.ToString()).ToArray());
        }

        /// <summary>
        /// Requires the user to have all of the specified permissions.
        /// </summary>
        /// <param name="permissions">The required permissions (user must have all).</param>
        /// <returns>The route handler builder for chaining.</returns>
        public RouteHandlerBuilder HasPermissions(params string[] permissions)
        {
            ArgumentNullException.ThrowIfNull(permissions);

            if (permissions.Length == 0)
                throw new ArgumentException("At least one permission must be specified.", nameof(permissions));

            var policyName = $"HasPermission:{string.Join(",", permissions)}";
            return builder.RequireAuthorization(policyName);
        }

        /// <summary>
        /// Requires the user to have all of the specified permissions.
        /// </summary>
        public RouteHandlerBuilder HasPermissions(params AppPermission[] permissions)
        {
            return builder.HasPermissions(permissions.Select(p => p.GetDescription()).ToArray());
        }

        /// <summary>
        /// Requires the user to have the specified permission.
        /// </summary>
        public RouteHandlerBuilder HasPermissions(AppPermission permission)
        {
            return builder.HasPermissions(permission.GetDescription());
        }

        /// <summary>
        /// Requires the user to have at least one of the specified permissions.
        /// </summary>
        /// <param name="permissions">The permissions (user must have at least one).</param>
        /// <returns>The route handler builder for chaining.</returns>
        public RouteHandlerBuilder HasAnyPermission(params string[] permissions)
        {
            ArgumentNullException.ThrowIfNull(permissions);

            if (permissions.Length == 0)
                throw new ArgumentException("At least one permission must be specified.", nameof(permissions));

            var policyName = $"HasAnyPermission:{string.Join(",", permissions)}";
            return builder.RequireAuthorization(policyName);
        }

        /// <summary>
        /// Requires the user to have at least one of the specified permissions.
        /// </summary>
        public RouteHandlerBuilder HasAnyPermission(params AppPermission[] permissions)
        {
            return builder.HasAnyPermission(permissions.Select(p => p.GetDescription()).ToArray());
        }
    }
}
