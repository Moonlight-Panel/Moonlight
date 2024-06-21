namespace Moonlight.Core.Attributes;

public class ApiPermissionAttribute : Attribute
{
    public string Permission { get; set; }
    
    public ApiPermissionAttribute(string permission)
    {
        Permission = permission;
    }
}