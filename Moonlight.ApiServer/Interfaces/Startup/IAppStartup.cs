namespace Moonlight.ApiServer.Interfaces.Startup;

public interface IAppStartup
{
    public Task BuildApp(IHostApplicationBuilder builder);
    public Task ConfigureApp(IApplicationBuilder app);
}