namespace T3mmyvsa.Features.Roles.CreateRole;

public class CreateRoleCommandHandler(RoleManager<IdentityRole> roleManager)
    : ICommandHandler<CreateRoleCommand, CreateRoleResponse>
{
    public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await roleManager.RoleExistsAsync(request.RoleName))
        {
            throw new InvalidOperationException("Role already exists.");
        }

        var result = await roleManager.CreateAsync(new IdentityRole(request.RoleName));
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var role = await roleManager.FindByNameAsync(request.RoleName);
        return new CreateRoleResponse(role!.Id, role.Name!);
    }
}
