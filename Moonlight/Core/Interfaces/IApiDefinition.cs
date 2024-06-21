namespace Moonlight.Core.Interfaces;

public interface IApiDefinition
{
    public string GetId();
    public string GetName();
    public string GetVersion();
    public string[] GetPermissions();
}