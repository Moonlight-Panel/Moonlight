using Moonlight.Core.Models.Abstractions.Services;

namespace Moonlight.Core.Plugins.Contexts;

public class PluginContext
{
    public IServiceCollection Services { get; set; }
    public IServiceProvider Provider { get; set; }
    public IServiceScope Scope { get; set; }
    public WebApplicationBuilder WebApplicationBuilder { get; set; }
    public WebApplication WebApplication { get; set; }
    public List<Action> PreInitTasks = new();
    public List<Action> PostInitTasks = new();
    public Action<ServiceViewContext>? BuildUserServiceView { get; set; } = null;
    public Action<ServiceViewContext>? BuildAdminServiceView { get; set; } = null;
}