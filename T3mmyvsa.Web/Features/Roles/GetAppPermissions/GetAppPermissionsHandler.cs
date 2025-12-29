using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Roles.GetAppPermissions;

public class GetAppPermissionsHandler : IQueryHandler<GetAppPermissionsQuery, List<AppPermissionResponse>>
{
    public Task<List<AppPermissionResponse>> Handle(GetAppPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = Enum.GetValues<AppPermission>()
            .Select(p =>
            {
                var value = p.GetDescription();
                var group = value.Split('.').FirstOrDefault() ?? "General";
                return new AppPermissionResponse(p.ToString(), value, group);
            })
            .ToList();

        return Task.FromResult(permissions);
    }
}
