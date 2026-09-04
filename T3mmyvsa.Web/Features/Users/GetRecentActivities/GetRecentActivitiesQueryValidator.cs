using FluentValidation;

namespace T3mmyvsa.Features.Users.GetRecentActivities;

public sealed class GetRecentActivitiesQueryValidator : AbstractValidator<GetRecentActivitiesQuery>
{
    public GetRecentActivitiesQueryValidator()
    {
        RuleFor(x => x.UserId).MaximumLength(450).When(x => x.UserId is not null);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).When(x => x.Page.HasValue);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
    }
}
