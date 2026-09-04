using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class CorsSettingsValidator : AbstractValidator<CorsSettings>
{
    public CorsSettingsValidator()
    {
        RuleForEach(x => x.AllowedOrigins)
            .Must(origin => Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                            origin != "*")
            .WithMessage("CORS origins must be explicit absolute HTTP/HTTPS origins; wildcard origins are not permitted.");

        RuleFor(x => x.AllowedMethods)
            .NotEmpty();

        RuleFor(x => x.AllowedHeaders)
            .NotEmpty();

        RuleFor(x => x.PreflightMaxAgeSeconds)
            .InclusiveBetween(0, 86400);

        RuleFor(x => x.AllowedOrigins)
            .NotEmpty()
            .When(x => x.AllowCredentials)
            .WithMessage("Credentialed CORS requires at least one explicit allowed origin.");
    }
}
