using System.Net;
using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class ProxySettingsValidator : AbstractValidator<ProxySettings>
{
    public ProxySettingsValidator()
    {
        RuleFor(x => x.ForwardLimit)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.KnownProxies)
            .NotEmpty()
            .When(x => x.Enabled)
            .WithMessage("At least one trusted proxy IP must be configured when forwarded headers are enabled.");

        RuleForEach(x => x.KnownProxies)
            .Must(value => IPAddress.TryParse(value, out _))
            .WithMessage("Known proxy entries must be valid IP addresses.");
    }
}
