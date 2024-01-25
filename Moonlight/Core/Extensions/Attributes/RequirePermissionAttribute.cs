using Moonlight.Core.Models.Enums;

namespace Moonlight.Core.Extensions.Attributes;

public class RequirePermissionAttribute : Attribute
{
    public int PermissionInteger = 0;

    public RequirePermissionAttribute(){}
    
    public RequirePermissionAttribute(int perms)
    {
        PermissionInteger = perms;
    }

    public RequirePermissionAttribute(Permission permission)
    {
        PermissionInteger = (int)permission;
    }
}