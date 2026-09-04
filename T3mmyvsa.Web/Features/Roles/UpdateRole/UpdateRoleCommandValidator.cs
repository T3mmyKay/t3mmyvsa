using FluentValidation;

namespace T3mmyvsa.Features.Roles.UpdateRole;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RoleName).NotEmpty().Length(2, 100);
    }
}
