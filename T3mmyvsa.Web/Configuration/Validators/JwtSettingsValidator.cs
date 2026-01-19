using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class JwtSettingsValidator : AbstractValidator<JwtSettings>
{
    public JwtSettingsValidator()
    {
        RuleFor(x => x.Secret)
            .NotEmpty()
            .WithMessage($"{nameof(JwtSettings.Secret)} is required")
            .MinimumLength(32)
            .WithMessage($"{nameof(JwtSettings.Secret)} must be at least 32 characters for security");

        RuleFor(x => x.ValidIssuer)
            .NotEmpty()
            .WithMessage($"{nameof(JwtSettings.ValidIssuer)} is required");

        RuleFor(x => x.ValidAudience)
            .NotEmpty()
            .WithMessage($"{nameof(JwtSettings.ValidAudience)} is required");

        RuleFor(x => x.TokenValidityInMinutes)
            .GreaterThan(0)
            .WithMessage($"{nameof(JwtSettings.TokenValidityInMinutes)} must be greater than 0");

        RuleFor(x => x.RefreshTokenValidityInDays)
            .GreaterThan(0)
            .WithMessage($"{nameof(JwtSettings.RefreshTokenValidityInDays)} must be greater than 0");
    }
}
