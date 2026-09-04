using FluentValidation;

namespace T3mmyvsa.Features.Roles.ManagePermissions;

public sealed class GetRolePermissionsQueryValidator : AbstractValidator<GetRolePermissionsQuery>
{
    public GetRolePermissionsQueryValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
