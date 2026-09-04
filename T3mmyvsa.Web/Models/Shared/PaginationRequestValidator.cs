using FluentValidation;

namespace T3mmyvsa.Models.Shared;

public sealed class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).When(x => x.Page.HasValue);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
    }
}
