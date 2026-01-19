using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class MailSettingsValidator : AbstractValidator<MailSettings>
{
    public MailSettingsValidator()
    {
        RuleFor(x => x.Host)
            .NotEmpty()
            .WithMessage($"{nameof(MailSettings.Host)} is required");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535)
            .WithMessage($"{nameof(MailSettings.Port)} must be between 1 and 65535");

        RuleFor(x => x.Mail)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Mail))
            .WithMessage($"{nameof(MailSettings.Mail)} must be a valid email address");
    }
}
