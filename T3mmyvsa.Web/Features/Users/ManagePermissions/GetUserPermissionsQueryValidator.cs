using FluentValidation;

namespace T3mmyvsa.Features.Users.ManagePermissions;

public sealed class GetUserPermissionsQueryValidator : AbstractValidator<GetUserPermissionsQuery>
{
    public GetUserPermissionsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
