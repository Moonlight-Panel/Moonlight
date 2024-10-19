using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MoonCore.Blazor.Tailwind.Extensions;
using MoonCore.Blazor.Tailwind.Forms;
using MoonCore.Blazor.Tailwind.Forms.Components;
using MoonCore.Blazor.Tailwind.Services;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Models;
using MoonCore.PluginFramework.Services;
using Moonlight.Client.Implementations;
using Moonlight.Client.Interfaces;
using Moonlight.Client.Services;
using Moonlight.Client.UI;
using Moonlight.Shared.Http.Requests.Auth;

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

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var result = new HttpApiClient(httpClient);

    result.UseBearerTokenConsumer(async () =>
    {
        var cookieService = sp.GetRequiredService<CookieService>();

        return new TokenConsumer(
            await cookieService.GetValue("kms-access", "x"),
            await cookieService.GetValue("kms-refresh", "x"),
            DateTimeOffset.FromUnixTimeSeconds(long.Parse(await cookieService.GetValue("kms-timestamp", "0"))).UtcDateTime,
            async refreshToken =>
            {
                await httpClient.PostAsync("api/auth/refresh", new StringContent(
                    JsonSerializer.Serialize(new RefreshRequest()
                    {
                        RefreshToken = refreshToken
                    }), new MediaTypeHeaderValue("application/json")
                ));

                return new TokenPair()
                {
                    AccessToken = await cookieService.GetValue("kms-access", "x"),
                    RefreshToken = await cookieService.GetValue("kms-refresh", "x")
                };
            }
        );
    });

    return result;
});

builder.Services.AddMoonCoreBlazorTailwind();
builder.Services.AddScoped<WindowService>();

builder.Services.AutoAddServices<Program>();

FormComponentRepository.Set<string, StringComponent>();
FormComponentRepository.Set<int, IntComponent>();

// Implementation service
var implementationService = new ImplementationService();

implementationService.Register<ISidebarItemProvider, DefaultSidebarItemProvider>();

var authUiHandler = new AuthenticationUiHandler();
implementationService.Register<IAppScreen>(authUiHandler);
implementationService.Register<IAppLoader>(authUiHandler);

builder.Services.AddSingleton(implementationService);

var app = builder.Build();

await app.RunAsync();