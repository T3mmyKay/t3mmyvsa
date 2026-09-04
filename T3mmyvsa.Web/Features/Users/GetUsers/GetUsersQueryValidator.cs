using FluentValidation;

namespace T3mmyvsa.Features.Users.GetUsers;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.Search).MaximumLength(200).When(x => x.Search is not null);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).When(x => x.Page.HasValue);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).When(x => x.PageSize.HasValue);
    }
}
