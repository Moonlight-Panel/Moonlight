namespace Moonlight.App.Perms;

public static class Permissions
{
    public static Permission AdminDashboard = new()
    {
        Index = 0,
        Name = "Admin dashboard",
        Description = "See basic information about growth and status of the moonlight instance"
    };

    public static Permission? FromString(string name)
    {
        var type = typeof(Permissions);

        var field = type
            .GetFields()
            .FirstOrDefault(x => x.FieldType == typeof(Permission) && x.Name == name);

        if (field != null)
        {
            var value = field.GetValue(null);
            return value as Permission;
        }

        return null;
    }

    public static Permission[] GetAllPermissions()
    {
        var type = typeof(Permissions);

        return type
            .GetFields()
            .Where(x => x.FieldType == typeof(Permission))
            .Select(x => (x.GetValue(null) as Permission)!)
            .ToArray();
    }
}