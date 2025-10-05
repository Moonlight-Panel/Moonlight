using Microsoft.AspNetCore.Builder;

namespace Moonlight.ApiServer.Plugins;

public interface IPluginStartup
{
    public void AddPlugin(WebApplicationBuilder builder);
    public void UsePlugin(WebApplication app);
    public void MapPlugin(WebApplication app);
}