using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Runtime;
using Moonlight.ApiServer.Startup;

var pluginLoader = new PluginLoader();
pluginLoader.Initialize();

var builder = WebApplication.CreateBuilder(args);

builder.AddMoonlight(pluginLoader.Instances);

var app = builder.Build();

app.UseMoonlight(pluginLoader.Instances);

// Add frontend
var configuration = AppConfiguration.CreateEmpty();
builder.Configuration.Bind(configuration);

// Handle setup of wasm app hosting in the runtime
// so the Moonlight.ApiServer doesn't need the wasm package
if (configuration.Frontend.EnableHosting)
{
    if (app.Environment.IsDevelopment())
        app.UseWebAssemblyDebugging();

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
}

app.MapMoonlight(pluginLoader.Instances);


await app.RunAsync();