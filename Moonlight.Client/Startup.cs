using System.Reflection;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MoonCore.Blazor.Extensions;
using MoonCore.Blazor.Services;
using MoonCore.Blazor.Tailwind.Extensions;
using MoonCore.Blazor.Tailwind.Forms;
using MoonCore.Blazor.Tailwind.Forms.Components;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.PluginFramework.Extensions;
using Moonlight.Client.Interfaces;
using Moonlight.Client.Services;
using Moonlight.Client.UI;
using Moonlight.Client.UI.Forms;

namespace Moonlight.Client;

public class Startup
{
    public static async Task Run(string[] args, Assembly[] assemblies)
    {
        // Build pre run logger
        var providers = LoggerBuildHelper.BuildFromConfiguration(configuration =>
        {
            configuration.Console.Enable = true;
            configuration.Console.EnableAnsiMode = true;
            configuration.FileLogging.Enable = false;
        });

        using var loggerFactory = new LoggerFactory(providers);
        var logger = loggerFactory.CreateLogger("Startup");

        // Fancy start console output... yes very fancy :>
        Console.Write("Running ");

        var rainbow = new Crayon.Rainbow(0.5);
        foreach (var c in "Moonlight")
        {
            Console.Write(
                rainbow
                    .Next()
                    .Bold()
                    .Text(c.ToString())
            );
        }

        Console.WriteLine();

        // Building app
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Configure application logging
        builder.Logging.ClearProviders();
        builder.Logging.AddProviders(providers);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.AddTokenAuthentication();
        builder.AddOAuth2();

/*
builder.Services.AddScoped(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var localStorageService = sp.GetRequiredService<LocalStorageService>();
    var result = new HttpApiClient(httpClient);

    result.AddLocalStorageTokenAuthentication(localStorageService, async refreshToken =>
    {
        try
        {
            var httpApiClient = new HttpApiClient(httpClient);

            var response = await httpApiClient.PostJson<RefreshResponse>(
                "api/auth/refresh",
                new RefreshRequest()
                {
                    RefreshToken = refreshToken
                }
            );

            return (new TokenPair()
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken
            }, response.ExpiresAt);
        }
        catch (HttpApiException)
        {
            return (new TokenPair()
            {
                AccessToken = "unset",
                RefreshToken = "unset"
            }, DateTime.MinValue);
        }
    });

    return result;
});*/

        builder.Services.AddMoonCoreBlazorTailwind();
        builder.Services.AddScoped<WindowService>();
        builder.Services.AddScoped<LocalStorageService>();

        builder.Services.AutoAddServices<Startup>();

        FormComponentRepository.Set<string, StringComponent>();
        FormComponentRepository.Set<int, IntComponent>();
        FormComponentRepository.Set<DateTime, DateComponent>();

        // Interface service
        builder.Services.AddPlugins(configuration =>
        {
            configuration.AddAssembly(typeof(Startup).Assembly);
            configuration.AddAssemblies(assemblies);

            configuration.AddInterface<IAppLoader>();
            configuration.AddInterface<IAppScreen>();

            configuration.AddInterface<ISidebarItemProvider>();
        });

        var app = builder.Build();

        await app.RunAsync();
    }
}