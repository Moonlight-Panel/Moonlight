using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Moonlight.Client.Interfaces;

public interface IAppStartup
{
    public Task BuildApp(WebAssemblyHostBuilder builder);
    public Task ConfigureApp(WebAssemblyHost app);
}