namespace T3mmyvsa.Features.Roles.CreateRole;

public class CreateRoleCommandHandler(RoleManager<IdentityRole> roleManager)
    : ICommandHandler<CreateRoleCommand, CreateRoleResponse>
{
    public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = request.RoleName.Trim();
        if (await roleManager.RoleExistsAsync(roleName))
        {
            throw new InvalidOperationException("Role already exists.");
        }

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var role = await roleManager.FindByNameAsync(roleName)
            ?? throw new InvalidOperationException("Role was created but could not be reloaded.");
        return new CreateRoleResponse(role.Id, role.Name!);
    }
}
