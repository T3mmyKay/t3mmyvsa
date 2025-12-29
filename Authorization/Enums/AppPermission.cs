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
    
    [Description("Users.View")]
    UsersView,
    
    [Description("Users.ManageRoles")]
    UsersManageRoles,

    [Description("Users.ViewActivity")]
    UsersViewActivity
}
