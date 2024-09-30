using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MoonCore.Blazor.Tailwind.Extensions;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.PluginFramework.Services;
using Moonlight.Client.Implementations;
using Moonlight.Client.Interfaces;
using Moonlight.Client.UI;

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
builder.Services.AddScoped(sp => new HttpApiClient(sp.GetRequiredService<HttpClient>()));

builder.Services.AddMoonCoreBlazorTailwind();

// Implementation service
var implementationService = new ImplementationService();

implementationService.Register<ISidebarItemProvider, DefaultSidebarItemProvider>();

builder.Services.AddSingleton(implementationService);

await builder.Build().RunAsync();