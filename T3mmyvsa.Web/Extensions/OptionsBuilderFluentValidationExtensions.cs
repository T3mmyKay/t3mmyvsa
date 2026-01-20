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

    /// <summary>
    /// Configuration extension that adds options with FluentValidation and ValidateOnStart
    /// </summary>
    /// <typeparam name="TOptions">The options type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configurationSection">The configuration section name to bind to</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOptionsWithFluentValidation<TOptions>(
        this IServiceCollection services,
        string configurationSection)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .BindConfiguration(configurationSection)
            .ValidateFluentValidation()
            .ValidateOnStart();

        return services;
    }
}
