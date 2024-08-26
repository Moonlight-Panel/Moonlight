using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Client;
using Moonlight.Client.App.Implementations;
using Moonlight.Client.App.Implementations.AdminDashboardCards;
using Moonlight.Client.App.Interfaces;
using Moonlight.Client.App.Models.Forms;
using Moonlight.Client.App.PluginApi;
using Moonlight.Client.App.UI.Components.Forms.Components;

// Build pre run logger
var providers = LoggerBuildHelper.BuildFromConfiguration(configuration =>
{
    configuration.Console.Enable = true;
    configuration.Console.EnableAnsiMode = true;

    configuration.FileLogging.Enable = false;
});

var preLoggerFactory = new LoggerFactory(providers);
var logger = preLoggerFactory.CreateLogger("Startup");

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Default form components
SmartFormComponentTypeMap.Set<string, StringComponent>();
SmartFormComponentTypeMap.Set<int, IntComponent>();
SmartFormComponentTypeMap.Set<bool, SwitchComponent>();
SmartFormComponentTypeMap.Set<DateTime, DateComponent>();

// Plugins
var pluginService = new PluginService(preLoggerFactory.CreateLogger<PluginService>(), preLoggerFactory);
await pluginService.Load(builder.HostEnvironment.BaseAddress);

builder.Services.AddSingleton(pluginService);

builder.RootComponents.Add<BlazorApp>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("Moonlight.ServerAPI",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Moonlight.ServerAPI"));

builder.Services.AutoAddServices<Program>();
builder.Services.AddSingleton<EventService>();

builder.Logging.ClearProviders();
builder.Logging.AddProviders(providers);

// Own implementations
pluginService.RegisterImplementation<ISidebarItemProvider, DefaultSidebarItemProvider>();

pluginService.RegisterImplementation<IAppLoader, AuthenticationStateLoader>();
pluginService.RegisterImplementation<IAppScreen, AuthenticationScreen>();

pluginService.RegisterImplementation<IAdminDashboardCard, UserCounterCard>();

await pluginService.CallPlugins(x => x.OnAppBuilding(builder));

var app = builder.Build();

await pluginService.CallPlugins(x => x.OnAppConfiguring(app));

await app.RunAsync();