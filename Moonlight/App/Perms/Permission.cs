namespace Moonlight.App.Perms;

public class Permission
{
    public int Index { get; set; } = 0;
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public static implicit operator int(Permission permission) => permission.Index;
}