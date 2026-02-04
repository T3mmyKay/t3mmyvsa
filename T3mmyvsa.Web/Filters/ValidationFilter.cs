using FluentValidation;

namespace T3mmyvsa.Filters;

public class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument is null) continue;

            // 1. FluentValidation
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }
            }

            // 2. DataAnnotations (Fallback)
            var validationContextData = new System.ComponentModel.DataAnnotations.ValidationContext(argument);
            var validationResultsData = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            if (!Validator.TryValidateObject(argument, validationContextData, validationResultsData, true))
            {
                var errors = validationResultsData
                    .Where(r => r.ErrorMessage != null)
                    .GroupBy(
                        r => r.MemberNames.FirstOrDefault() ?? string.Empty,
                        r => r.ErrorMessage!
                    )
                    .ToDictionary(g => g.Key, g => g.ToArray());

                return Results.ValidationProblem(errors);
            }
        }

        return await next(context);
    }
}