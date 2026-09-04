using FluentValidation;

namespace T3mmyvsa.Features.Users.RemoveUserRole;

public sealed class RemoveRoleCommandValidator : AbstractValidator<RemoveRoleCommand>
{
    public RemoveRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(256);
    }
}
