using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class RateLimitPolicySettingsValidator : AbstractValidator<RateLimitPolicySettings>
{
    public RateLimitPolicySettingsValidator()
    {
        RuleFor(x => x.PermitLimit)
            .InclusiveBetween(1, 10000);

        RuleFor(x => x.WindowSeconds)
            .InclusiveBetween(1, 86400);
    }
}
