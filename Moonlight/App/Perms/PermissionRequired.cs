namespace Moonlight.App.Perms;

public class PermissionRequired : Attribute
{
    public string Name { get; private set; }

    public PermissionRequired(string name)
    {
        Name = name;
    }
}