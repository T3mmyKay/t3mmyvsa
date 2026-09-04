using FluentValidation;

namespace T3mmyvsa.Features.Users.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEqual(Guid.Empty);
        RuleFor(x => x.FirstName).NotEmpty().Length(2, 100);
        RuleFor(x => x.LastName).NotEmpty().Length(2, 100);
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(32)
            .Matches(@"^[0-9+() .-]+$").WithMessage("Phone number contains invalid characters.");
    }
}
