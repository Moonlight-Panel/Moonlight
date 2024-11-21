using System.Reflection;

namespace Moonlight.Client.Services;

public class ApplicationAssemblyService
{
    public Assembly[] AdditionalAssemblies { get; set; }
    public Assembly[] PluginAssemblies { get; set; }
    public Assembly[] NavigationAssemblies => PluginAssemblies.Concat(AdditionalAssemblies).ToArray();
}