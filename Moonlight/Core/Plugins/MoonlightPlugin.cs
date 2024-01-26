using Moonlight.Core.Plugins.Contexts;

namespace Moonlight.Core.Plugins;

public abstract class MoonlightPlugin
{
    public PluginContext Context { get; set; }
    public abstract Task Enable();
    public abstract Task Disable();
}