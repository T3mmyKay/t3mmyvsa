using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class HangfireSettingsValidator : AbstractValidator<HangfireSettings>
{
    public HangfireSettingsValidator()
    {
        RuleFor(x => x.StorageProvider)
            .NotEmpty()
            .Must(IsSupportedStorageProvider)
            .WithMessage(
                "Hangfire StorageProvider must be 'inherit', 'sqlserver'/'mssql', or 'postgresql'/'postgres'/'pgsql'.");

        RuleFor(x => x.ConnectionStringName)
            .Matches("^[A-Za-z][A-Za-z0-9_-]*$")
            .When(x => !string.IsNullOrWhiteSpace(x.ConnectionStringName));

        RuleFor(x => x.SchemaName)
            .NotEmpty()
            .Matches("^[A-Za-z][A-Za-z0-9_]*$");

        RuleFor(x => x.WorkerCount)
            .InclusiveBetween(1, 256)
            .When(x => x.WorkerCount.HasValue);

        RuleFor(x => x.Queues)
            .NotEmpty();

        RuleForEach(x => x.Queues)
            .NotEmpty()
            .Matches("^[a-z0-9_-]+$");

        RuleFor(x => x.AutomaticRetryAttempts)
            .InclusiveBetween(0, 10);

        RuleFor(x => x.Dashboard)
            .SetValidator(new HangfireDashboardSettingsValidator());
    }

    private static bool IsSupportedStorageProvider(string? value)
    {
        if (string.Equals(value, "inherit", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return DatabaseProviders.TryNormalize(value, out var provider) &&
               provider is DatabaseProviders.SqlServer or DatabaseProviders.PostgreSql;
    }
}
