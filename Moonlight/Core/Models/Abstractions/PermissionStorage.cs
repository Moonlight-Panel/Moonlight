using Moonlight.Core.Models.Enums;

namespace Moonlight.Core.Models.Abstractions;

public class PermissionStorage
{
    public readonly int PermissionInteger;

    public PermissionStorage(int permissionInteger)
    {
        PermissionInteger = permissionInteger;
    }

    public Permission[] Permissions => GetPermissions();
    
    public Permission[] GetPermissions()
    {
        return GetAllPermissions()
            .Where(x => (int)x <= PermissionInteger)
            .ToArray();
    }

    public static Permission[] GetAllPermissions()
    {
        return Enum.GetValues<Permission>();
    }

    public static Permission GetFromInteger(int id)
    {
        return GetAllPermissions().First(x => (int)x == id);
    }

    public bool this[Permission permission] => Permissions.Contains(permission);
}