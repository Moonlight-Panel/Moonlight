namespace Moonlight.Client.App.Attributes;

public class RequirePermissionAttribute : Attribute
{
    public string Permission { get; set; }

    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
    }
}