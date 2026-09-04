using FluentValidation;

namespace T3mmyvsa.Configuration.Validators;

public sealed class HangfireSettingsValidator : AbstractValidator<HangfireSettings>
{
    public HangfireSettingsValidator()
    {
        RuleFor(x => x.ConnectionStringName)
            .NotEmpty();

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
}
