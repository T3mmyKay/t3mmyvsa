namespace T3mmyvsa.Features.Roles.GetRoles;

public class GetRolesQueryHandler(RoleManager<IdentityRole> roleManager)
    : IQueryHandler<GetRolesQuery, List<RoleResponse>>
{
    public async Task<List<RoleResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await roleManager.Roles
            .Select(r => new RoleResponse(r.Id, r.Name!))
            .ToListAsync(cancellationToken);

        return roles;
    }
}
