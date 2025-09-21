using Microsoft.Extensions.DependencyInjection;
using MoonCore.Blazor.FlyonUi;
using MoonCore.Extensions;
using MoonCore.Helpers;
using Moonlight.Client.Services;
using Moonlight.Client.UI;

namespace Moonlight.Client.Startup;

public partial class Startup
{
    private Task RegisterBaseAsync()
    {
        WebAssemblyHostBuilder.RootComponents.Add<App>("#app");
        WebAssemblyHostBuilder.RootComponents.Add<HeadOutlet>("head::after");

        WebAssemblyHostBuilder.Services.AddScoped(_ =>
            new HttpClient
            {
                BaseAddress = new Uri(Configuration.ApiUrl)
            }
        );

        WebAssemblyHostBuilder.Services.AddScoped(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            return new HttpApiClient(httpClient);
        });

        WebAssemblyHostBuilder.Services.AddFileManagerOperations();
        WebAssemblyHostBuilder.Services.AddFlyonUiServices();

        WebAssemblyHostBuilder.Services.AddScoped<ThemeService>();

        WebAssemblyHostBuilder.Services.AutoAddServices<Startup>();

        return Task.CompletedTask;
    }
}