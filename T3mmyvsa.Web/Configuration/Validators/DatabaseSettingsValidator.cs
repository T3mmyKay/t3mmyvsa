using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class DatabaseSettingsValidator : AbstractValidator<DatabaseSettings>
{
    private static readonly string[] AllowedProviders = ["mssql", "mysql"];

    public DatabaseSettingsValidator()
    {
        RuleFor(x => x.DBProvider)
            .NotEmpty()
            .WithMessage($"{nameof(DatabaseSettings.DBProvider)} is required")
            .Must(provider => AllowedProviders.Contains(provider, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"{nameof(DatabaseSettings.DBProvider)} must be one of: {string.Join(", ", AllowedProviders)}");
    }
}
