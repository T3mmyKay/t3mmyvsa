using FluentValidation;

namespace T3mmyvsa.Features.Users.GetUser;

public sealed class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
