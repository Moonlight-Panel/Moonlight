using Moonlight.ApiServer.Helpers;

namespace Moonlight.ApiServer.Interfaces.Startup;

public interface IPluginStartup
{
    public Task BuildApplication(IHostApplicationBuilder builder);
    public Task ConfigureApplication(IApplicationBuilder app);
    public Task ConfigureDatabase(DatabaseContextCollection collection);
    public Task ConfigureEndpoints(IEndpointRouteBuilder routeBuilder);
}