using System.ComponentModel;

namespace T3mmyvsa.Authorization.Enums;

public enum AppPermission
{
    [Description("Roles.View")]
    RolesView,

    [Description("Roles.Create")]
    RolesCreate,

    [Description("Roles.Update")]
    RolesUpdate,

    [Description("Roles.Delete")]
    RolesDelete,

    [Description("Users.View")]
    UsersView,

    [Description("Users.Create")]
    UsersCreate,

    [Description("Users.Update")]
    UsersUpdate,

    [Description("Users.Delete")]
    UsersDelete,

    [Description("Users.ManageRoles")]
    UsersManageRoles,

    [Description("Users.ViewActivity")]
    UsersViewActivity,

    [Description("Users.Deactivate")]
    UsersDeactivate,

    [Description("Users.Activate")]
    UsersActivate,
}
