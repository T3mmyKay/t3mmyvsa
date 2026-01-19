using FluentValidation;
using Microsoft.Extensions.Options;

namespace T3mmyvsa.Configuration.Validators;

/// <summary>
/// Implements IValidateOptions using FluentValidation validators
/// </summary>
/// <typeparam name="TOptions">The options type to validate</typeparam>
public sealed class FluentValidateOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _name;

    public FluentValidateOptions(IServiceProvider serviceProvider, string? name)
    {
        _serviceProvider = serviceProvider;
        _name = name;
    }

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        // Skip if this is for a different named options instance
        if (_name is not null && _name != name)
        {
            return ValidateOptionsResult.Skip;
        }

        // Create a scope to resolve scoped validators
        using var scope = _serviceProvider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();
        
        var result = validator.Validate(options);

        if (result.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        var errors = result.Errors
            .Select(e => $"{typeof(TOptions).Name}.{e.PropertyName}: {e.ErrorMessage}");
        
        return ValidateOptionsResult.Fail(errors);
    }
}
