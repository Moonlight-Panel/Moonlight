using Microsoft.Extensions.DependencyInjection;
using MoonCore.Blazor.FlyonUi;
using MoonCore.Blazor.Services;
using MoonCore.Extensions;
using MoonCore.Helpers;
using Moonlight.Client.Services;
using Moonlight.Client.UI;

namespace Moonlight.Client.Startup;

public partial class Startup
{
    private Task RegisterBase()
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
            var httpApiClient = new HttpApiClient(httpClient);

            var localStorageService = sp.GetRequiredService<LocalStorageService>();

            httpApiClient.OnConfigureRequest += async request =>
            {
                var accessToken = await localStorageService.GetString("AccessToken");

                if (string.IsNullOrEmpty(accessToken))
                    return;

                request.Headers.Add("Authorization", $"Bearer {accessToken}");
            };

            return httpApiClient;
        });

        WebAssemblyHostBuilder.Services.AddScoped<WindowService>();
        WebAssemblyHostBuilder.Services.AddFileManagerOperations();
        WebAssemblyHostBuilder.Services.AddFlyonUiServices();
        WebAssemblyHostBuilder.Services.AddScoped<LocalStorageService>();

        WebAssemblyHostBuilder.Services.AddScoped<ThemeService>();

        WebAssemblyHostBuilder.Services.AutoAddServices<Startup>();

        return Task.CompletedTask;
    }
}