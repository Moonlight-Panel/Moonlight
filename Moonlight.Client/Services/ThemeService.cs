using MoonCore.Attributes;
using Moonlight.Shared.Misc;

namespace Moonlight.Client.Services;

[Singleton]
public class ThemeService
{
    public event Func<Task> OnRefresh;
    
    public Dictionary<string, string> Variables { get; private set; } = new();
    
    public ThemeService(FrontendConfiguration configuration)
    {
        // Load theme variables into the cache
        foreach (var themeVariable in configuration.Theme.Variables)
            Variables[themeVariable.Key] = themeVariable.Value;
    }

    public async Task Refresh()
    {
        if (OnRefresh != null)
            await OnRefresh.Invoke();
    }
}