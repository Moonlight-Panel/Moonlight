using System.Reflection;

namespace Moonlight.Core.Models.Abstractions.Feature;

public class PreInitContext
{
    public WebApplicationBuilder Builder { get; set; }
    public List<Assembly> DiAssemblies { get; set; } = new();
    public Dictionary<string, List<string>> Assets { get; set; } = new();
    
    public void EnableDependencyInjection<T>()
    {
        var assembly = typeof(T).Assembly;
        
        if(!DiAssemblies.Contains(assembly))
            DiAssemblies.Add(assembly);
    }

    public void AddAsset(string name, string path)
    {
        if (!Assets.ContainsKey(name))
            Assets[name] = new();
        
        Assets[name].Add(path);
    }
}