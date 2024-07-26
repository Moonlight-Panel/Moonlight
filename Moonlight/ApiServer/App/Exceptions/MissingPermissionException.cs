namespace Moonlight.ApiServer.App.Exceptions;

public class MissingPermissionException : Exception
{
    public string[] RequiredPermissions { get; set; } = [];
    
    public MissingPermissionException()
    {
    }
    
    public MissingPermissionException(string[] permissions)
    {
        RequiredPermissions = permissions;
    }

    public MissingPermissionException(string message) : base(message)
    {
    }

    public MissingPermissionException(string message, Exception inner) : base(message, inner)
    {
    }
}