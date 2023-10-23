namespace Moonlight.App.Plugins.Contexts;

public class PluginContext
{
    public IServiceCollection Services { get; set; }
    public IServiceProvider Provider { get; set; }
    public IServiceScope Scope { get; set; }
    public List<Action> PreInitTasks = new();
    public List<Action> PostInitTasks = new();
}