namespace Moonlight.ApiServer.Interfaces.Startup;

public interface IEndpointStartup
{
    public Task ConfigureEndpoints(IEndpointRouteBuilder routeBuilder);
}