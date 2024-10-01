namespace Moonlight.ApiServer.Exceptions;

public class MissingPermissionException : Exception
{
    public string Permission { get; set; }
    
    public MissingPermissionException(string permission)
    {
        Permission = permission;
    }
}