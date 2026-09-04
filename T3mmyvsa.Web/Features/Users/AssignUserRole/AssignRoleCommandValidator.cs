using FluentValidation;

namespace T3mmyvsa.Features.Users.AssignUserRole;

public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        // UserId is route-authoritative and is overwritten by the endpoint before dispatch.
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(256);
    }
}
