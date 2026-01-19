using Microsoft.Extensions.Options;
using T3mmyvsa.Configuration.Validators;

namespace T3mmyvsa.Extensions;

/// <summary>
/// Extension methods for OptionsBuilder to integrate FluentValidation
/// </summary>
public static class OptionsBuilderFluentValidationExtensions
{
    /// <summary>
    /// Adds FluentValidation-based validation for the options
    /// </summary>
    /// <typeparam name="TOptions">The options type</typeparam>
    /// <param name="builder">The options builder</param>
    /// <returns>The options builder for chaining</returns>
    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(
        this OptionsBuilder<TOptions> builder)
        where TOptions : class
    {
        builder.Services.AddSingleton<IValidateOptions<TOptions>>(
            serviceProvider => new FluentValidateOptions<TOptions>(
                serviceProvider, 
                builder.Name));

        return builder;
    }
}
