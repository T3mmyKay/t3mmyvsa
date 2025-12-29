namespace T3mmyvsa.Features.Roles.UpdateRole;

public class UpdateRoleCommandHandler(RoleManager<IdentityRole> roleManager)
    : ICommandHandler<UpdateRoleCommand, UpdateRoleResponse>
{
    public async Task<UpdateRoleResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.Id) ?? throw new KeyNotFoundException("Role not found.");
        role.Name = request.RoleName;
        var result = await roleManager.UpdateAsync(role);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return new UpdateRoleResponse(role.Id, role.Name!);
    }
}
