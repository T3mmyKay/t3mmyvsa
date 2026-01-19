using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class AppSettingsValidator : AbstractValidator<AppSettings>
{
    public AppSettingsValidator()
    {
        RuleFor(x => x.AppUrl)
            .NotEmpty()
            .WithMessage($"{nameof(AppSettings.AppUrl)} is required")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.AppUrl))
            .WithMessage($"{nameof(AppSettings.AppUrl)} must be a valid URL");
    }
}
