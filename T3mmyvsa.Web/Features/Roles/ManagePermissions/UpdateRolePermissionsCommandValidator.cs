using FluentValidation;
using T3mmyvsa.Authorization.Enums;
using T3mmyvsa.Extensions;

namespace T3mmyvsa.Features.Roles.ManagePermissions;

public sealed class UpdateRolePermissionsCommandValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    private static readonly HashSet<string> KnownPermissions = Enum.GetValues<AppPermission>()
        .Select(permission => permission.GetDescription())
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public UpdateRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Permissions)
            .NotNull()
            .Must(permissions => permissions is null || permissions.Count == permissions.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            .WithMessage("Permission values must be unique.");

        RuleForEach(x => x.Permissions)
            .NotEmpty()
            .Must(permission => KnownPermissions.Contains(permission))
            .WithMessage("Unknown permission '{PropertyValue}'.");
    }
}
