using System.Reflection;
using MoonCore.Extended.Helpers;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.PluginFramework.Extensions;
using MoonCore.Services;
using Moonlight.ApiServer;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Helpers;
using Moonlight.ApiServer.Http.Middleware;
using Moonlight.ApiServer.Interfaces.Auth;
using Moonlight.ApiServer.Interfaces.OAuth2;
using Moonlight.ApiServer.Interfaces.Startup;

// Cry about it
#pragma warning disable ASP0000

// Fancy start console output... yes very fancy :>
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

// Storage i guess
Directory.CreateDirectory(PathBuilder.Dir("storage"));

// Configuration
var configService = new ConfigService<AppConfiguration>(
    PathBuilder.File("storage", "config.json")
);

var config = configService.Get();

ApplicationStateHelper.SetConfiguration(configService);

// TODO: Load plugin/module assemblies

// Configure startup logger
var startupLoggerFactory = new LoggerFactory();

// TODO: Add direct extension method
var providers = LoggerBuildHelper.BuildFromConfiguration(configuration =>
{
    configuration.Console.Enable = true;
    configuration.Console.EnableAnsiMode = true;
    configuration.FileLogging.Enable = false;
});

startupLoggerFactory.AddProviders(providers);

var startupLogger = startupLoggerFactory.CreateLogger("Startup");

// Configure startup interfaces
var startupServiceCollection = new ServiceCollection();
startupServiceCollection.AddSingleton(configService);

startupServiceCollection.AddLogging(loggingBuilder => { loggingBuilder.AddProviders(providers); });

startupServiceCollection.AddPlugins(configuration =>
{
    // Configure startup interfaces
    configuration.AddInterface<IAppStartup>();
    configuration.AddInterface<IDatabaseStartup>();
    configuration.AddInterface<IEndpointStartup>();

    // Configure assemblies to scan
    configuration.AddAssembly(Assembly.GetEntryAssembly()!);
}, startupLogger);


var startupServiceProvider = startupServiceCollection.BuildServiceProvider();
var appStartupInterfaces = startupServiceProvider.GetRequiredService<IAppStartup[]>();

// Start the actual app
var builder = WebApplication.CreateBuilder(args);

await Startup.ConfigureLogging(builder);

await Startup.ConfigureDatabase(
    builder,
    startupLoggerFactory,
    startupServiceProvider.GetRequiredService<IDatabaseStartup[]>()
);

// Call interfaces
foreach (var startupInterface in appStartupInterfaces)
{
    try
    {
        await startupInterface.BuildApp(builder);
    }
    catch (Exception e)
    {
        startupLogger.LogCritical(
            "An unhandled error occured while processing BuildApp call for interface '{interfaceName}': {e}",
            startupInterface.GetType().FullName,
            e
        );
    }
}

builder.Services.AddControllers();
builder.Services.AddSingleton(configService);
builder.Services.AutoAddServices<Program>();
builder.Services.AddSingleton<TokenHelper>();
builder.Services.AddHttpClient();

await Startup.ConfigureTokenAuthentication(builder, config);
await Startup.ConfigureOAuth2(builder, startupLogger, config);

// Implementation service
builder.Services.AddPlugins(configuration =>
{
    configuration.AddInterface<IOAuth2Provider>();
    configuration.AddInterface<IAuthInterceptor>();

    configuration.AddAssembly(Assembly.GetEntryAssembly()!);
}, startupLogger);

var app = builder.Build();

await Startup.PrepareDatabase(app);

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<ApiErrorMiddleware>();

await Startup.UseTokenAuthentication(app);

// Call interfaces
foreach (var startupInterface in appStartupInterfaces)
{
    try
    {
        await startupInterface.ConfigureApp(app);
    }
    catch (Exception e)
    {
        startupLogger.LogCritical(
            "An unhandled error occured while processing ConfigureApp call for interface '{interfaceName}': {e}",
            startupInterface.GetType().FullName,
            e
        );
    }
}

app.UseMiddleware<AuthorizationMiddleware>();

// Call interfaces
var endpointStartupInterfaces = startupServiceProvider.GetRequiredService<IEndpointStartup[]>();

foreach (var endpointStartup in endpointStartupInterfaces)
{
    try
    {
        await endpointStartup.ConfigureEndpoints(app);
    }
    catch (Exception e)
    {
        startupLogger.LogCritical(
            "An unhandled error occured while processing ConfigureEndpoints call for interface '{interfaceName}': {e}",
            endpointStartup.GetType().FullName,
            e
        );
    }
}

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();