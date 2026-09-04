using FluentValidation;

namespace T3mmyvsa.Features.Auth.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(100)
            .Matches(@"\d").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.FirstName).NotEmpty().Length(2, 100);
        RuleFor(x => x.LastName).NotEmpty().Length(2, 100);
    }
}
