using FluentValidation;

namespace T3mmyvsa.Features.Users.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().Length(2, 100);
        RuleFor(x => x.LastName).NotEmpty().Length(2, 100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(32)
            .Matches(@"^[0-9+() .-]+$").WithMessage("Phone number contains invalid characters.");
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(100)
            .Matches(@"\d").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.Role).NotEmpty().MaximumLength(256);
    }
}
