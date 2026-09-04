using FluentValidation;

namespace T3mmyvsa.Features.Roles.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().Length(2, 100);
    }
}
