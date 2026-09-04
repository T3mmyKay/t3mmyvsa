using FluentValidation;

namespace T3mmyvsa.Features.Users.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).Length(2, 100).When(x => x.FirstName is not null);
        RuleFor(x => x.LastName).Length(2, 100).When(x => x.LastName is not null);
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(32)
            .Matches(@"^[0-9+() .-]+$")
            .WithMessage("Phone number contains invalid characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
