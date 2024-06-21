using Moonlight.Core.Interfaces;

namespace Moonlight.Core.Implementations.ApiDefinition;

public class InternalApiDefinition : IApiDefinition
{
    public string GetId() => "internal";

    public string GetName() => "Internal API";

    public string GetVersion() => "v2";

    public string[] GetPermissions() => [];
}