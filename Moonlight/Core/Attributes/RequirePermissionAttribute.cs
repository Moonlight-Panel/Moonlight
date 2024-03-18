namespace Moonlight.Core.Attributes;

public class RequirePermissionAttribute : Attribute
{
    public int Level { get; set; }

    public RequirePermissionAttribute(int level)
    {
        Level = level;
    }
}