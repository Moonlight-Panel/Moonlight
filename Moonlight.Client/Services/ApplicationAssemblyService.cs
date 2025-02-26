using System.Reflection;

namespace Moonlight.Client.Services;

public class ApplicationAssemblyService
{
    public List<Assembly> Assemblies { get; set; } = new();
}