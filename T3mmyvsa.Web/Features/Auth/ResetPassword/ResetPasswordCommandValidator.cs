using FluentValidation;

namespace T3mmyvsa.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(100)
            .Matches(@"\d").WithMessage("Password must contain at least one digit.");
    }
}
