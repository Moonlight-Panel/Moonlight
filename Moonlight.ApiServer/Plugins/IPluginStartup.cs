namespace Moonlight.ApiServer.Plugins;

public interface IPluginStartup
{
    public Task BuildApplication(IServiceProvider serviceProvider, IHostApplicationBuilder builder);
    public Task ConfigureApplication(IServiceProvider serviceProvider, IApplicationBuilder app);
    public Task ConfigureEndpoints(IServiceProvider serviceProvider, IEndpointRouteBuilder routeBuilder);
}