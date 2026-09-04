using FluentValidation;

namespace T3mmyvsa.Features.Auth.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(100)
            .Matches(@"\d").WithMessage("Password must contain at least one digit.")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from the current password.");
    }
}
