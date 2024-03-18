using Moonlight.Core.Services;

namespace Moonlight.Core.Models.Abstractions.Plugins;

public abstract class MoonlightPlugin
{
    public string Name { get; set; } = "";
    public string Author { get; set; } = "";
    public string IssueTracker { get; set; } = "";
    
    public PluginService Plugin { get; set; }

    public virtual Task OnPreInitialized(WebApplicationBuilder builder) => Task.CompletedTask;
    public virtual Task OnInitialized(WebApplication application) => Task.CompletedTask;
}