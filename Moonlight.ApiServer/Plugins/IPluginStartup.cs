using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace Moonlight.ApiServer.Plugins;

public interface IPluginStartup
{
    public Task BuildApplicationAsync(IServiceProvider serviceProvider, IHostApplicationBuilder builder);
    public Task ConfigureApplicationAsync(IServiceProvider serviceProvider, IApplicationBuilder app);
    public Task ConfigureEndpointsAsync(IServiceProvider serviceProvider, IEndpointRouteBuilder routeBuilder);
}