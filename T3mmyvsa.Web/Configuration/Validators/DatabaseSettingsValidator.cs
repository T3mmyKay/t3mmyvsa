using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class DatabaseSettingsValidator : AbstractValidator<DatabaseSettings>
{
    public DatabaseSettingsValidator()
    {
        RuleFor(x => x.Provider)
            .Must((settings, _) => DatabaseProviders.TryNormalize(settings.ConfiguredProvider, out _))
            .WithMessage(
                $"Database provider must be one of: {DatabaseProviders.SqlServer}, {DatabaseProviders.PostgreSql}, {DatabaseProviders.MySql}, {DatabaseProviders.Sqlite}.");

        RuleFor(x => x.ConnectionStringName)
            .NotEmpty()
            .Matches("^[A-Za-z][A-Za-z0-9_-]*$")
            .WithMessage("ConnectionStringName must be a valid configuration key segment.");
    }
}
