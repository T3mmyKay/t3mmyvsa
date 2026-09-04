using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class RateLimitSettingsValidator : AbstractValidator<RateLimitSettings>
{
    public RateLimitSettingsValidator()
    {
        RuleFor(x => x.Login).SetValidator(new RateLimitPolicySettingsValidator());
        RuleFor(x => x.Registration).SetValidator(new RateLimitPolicySettingsValidator());
        RuleFor(x => x.Recovery).SetValidator(new RateLimitPolicySettingsValidator());
        RuleFor(x => x.Refresh).SetValidator(new RateLimitPolicySettingsValidator());
    }
}
