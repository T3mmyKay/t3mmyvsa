namespace T3mmyvsa.Filters;

public class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument is null) continue;

            var validationContext = new ValidationContext(argument);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(argument, validationContext, validationResults, true))
            {
                var errors = validationResults
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
