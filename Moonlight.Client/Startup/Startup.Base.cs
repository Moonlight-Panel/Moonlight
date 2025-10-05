using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MoonCore.Blazor.FlyonUi;
using MoonCore.Blazor.FlyonUi.Exceptions;
using MoonCore.Extensions;
using MoonCore.Helpers;
using Moonlight.Client.Implementations;
using Moonlight.Client.Services;
using Moonlight.Client.UI;

namespace Moonlight.Client.Startup;

public static partial class Startup
{
    private static void AddBase(this WebAssemblyHostBuilder builder)
    {
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ =>
            new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            }
        );

        builder.Services.AddScoped(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            return new HttpApiClient(httpClient);
        });

        builder.Services.AddFileManagerOperations();
        builder.Services.AddFlyonUiServices();

        builder.Services.AddScoped<ThemeService>();

        builder.Services.AutoAddServices<IAssemblyMarker>();
        
        builder.Services.AddScoped<IGlobalErrorFilter, LogErrorFilter>();
    }
}