using BlazorTable;
using MoonCore.Helpers;
using MoonCore.Extensions;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database;
using Moonlight.Core.Services;
using Moonlight.Features.Servers.Actions;
using Moonlight.Features.Servers.Http.Middleware;
using Moonlight.Features.ServiceManagement.Entities.Enums;
using Moonlight.Features.ServiceManagement.Services;

var builder = WebApplication.CreateBuilder(args);

var configService = new ConfigService<ConfigV1>(
    PathBuilder.File("storage", "config.json")
);

Directory.CreateDirectory(PathBuilder.Dir("storage"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));

// Setup logger

var now = DateTime.UtcNow;
var logPath = PathBuilder.File("storage", "logs",
    $"moonlight-{now.Day}-{now.Month}-{now.Year}---{now.Hour}-{now.Minute}.log");

Logger.Setup(
    logPath: logPath,
    logInFile: true,
    logInConsole: true
);

// Init plugin system
var pluginService = new PluginService();
builder.Services.AddSingleton(pluginService);

await pluginService.Load(builder);
await pluginService.RunPreInit();

// TODO: Add automatic assembly scanning
// dependency injection registration
// using attributes

builder.Services.AddDbContext<DataContext>();

// Services
builder.Services.AddSingleton(configService);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddBlazorTable();

builder.Services.ConstructMoonCoreDi<Program>();

builder.Logging.MigrateToMoonCore();

var config =
    new ConfigurationBuilder().AddJsonString(
        "{\"LogLevel\":{\"Default\":\"Information\",\"Microsoft.AspNetCore\":\"Warning\"}}");
builder.Logging.AddConfiguration(config.Build());

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseWebSockets();

app.UseMiddleware<NodeMiddleware>();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Services.StartBackgroundServices<Program>();

var moonlightService = app.Services.GetRequiredService<MoonlightService>();
moonlightService.Application = app;
moonlightService.LogPath = logPath;

var serviceService = app.Services.GetRequiredService<ServiceDefinitionService>();
serviceService.Register<ServerServiceDefinition>(ServiceType.Server);

await pluginService.RunPrePost(app);

app.Run();