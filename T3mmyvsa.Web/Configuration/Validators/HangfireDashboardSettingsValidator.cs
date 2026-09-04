using System.Net;
using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class HangfireDashboardSettingsValidator : AbstractValidator<HangfireDashboardSettings>
{
    public HangfireDashboardSettingsValidator()
    {
        RuleFor(x => x.Path)
            .NotEmpty()
            .Must(path => path.StartsWith("/", StringComparison.Ordinal) && !path.Contains(' '))
            .WithMessage("Hangfire dashboard path must start with '/' and contain no spaces.");

        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MinimumLength(4);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(16);
        });

        RuleForEach(x => x.AllowedIpAddresses)
            .Must(value => IPAddress.TryParse(value, out _))
            .WithMessage("Hangfire dashboard allowed IP entries must be valid IP addresses.");
    }
}
