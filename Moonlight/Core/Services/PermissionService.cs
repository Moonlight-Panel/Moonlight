using MoonCore.Attributes;
using Moonlight.Core.Models;

namespace Moonlight.Core.Services;

// This service class is for the permission editor and can be used by features to let admins know what permission levels
// they check for

[Singleton]
public class PermissionService
{
    public readonly Dictionary<int, PermissionDefinition> Definitions = new();

    public Task Register(int level, PermissionDefinition definition)
    {
        if (Definitions.ContainsKey(level))
            throw new ArgumentException("A level with this value has already been registered");
        
        Definitions.Add(level, definition);
        
        return Task.CompletedTask;
    }
}