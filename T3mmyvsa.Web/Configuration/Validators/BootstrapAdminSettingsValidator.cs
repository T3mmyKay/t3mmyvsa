using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class BootstrapAdminSettingsValidator : AbstractValidator<BootstrapAdminSettings>
{
    public BootstrapAdminSettingsValidator()
    {
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(12)
                .MaximumLength(128);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);
        });
    }
}
