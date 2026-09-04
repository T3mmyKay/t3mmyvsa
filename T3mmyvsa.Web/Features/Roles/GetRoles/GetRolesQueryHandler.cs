using T3mmyvsa.Authorization.Enums;

namespace T3mmyvsa.Features.Roles.GetRoles;

public class GetRolesQueryHandler(RoleManager<IdentityRole> roleManager)
    : IQueryHandler<GetRolesQuery, List<RoleResponse>>
{
    private static readonly HashSet<string> SystemRoles = Enum.GetNames<AppRole>()
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public async Task<List<RoleResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await roleManager.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new { r.Id, r.Name })
            .ToListAsync(cancellationToken);

        return roles
            .Where(r => r.Name is not null)
            .Select(r => new RoleResponse(r.Id, r.Name!, SystemRoles.Contains(r.Name!)))
            .ToList();
    }
}
