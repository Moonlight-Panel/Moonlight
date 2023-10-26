using Moonlight.App.Plugins.Contexts;

namespace Moonlight.App.Plugins;

public abstract class MoonlightPlugin
{
    public PluginContext Context { get; set; }
    public abstract Task Enable();
    public abstract Task Disable();
}