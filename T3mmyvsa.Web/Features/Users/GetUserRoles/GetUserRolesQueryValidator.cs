using FluentValidation;

namespace T3mmyvsa.Features.Users.GetUserRoles;

public sealed class GetUserRolesQueryValidator : AbstractValidator<GetUserRolesQuery>
{
    public GetUserRolesQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
