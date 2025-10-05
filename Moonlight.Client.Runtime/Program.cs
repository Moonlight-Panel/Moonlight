using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Moonlight.Client.Runtime;
using Moonlight.Client.Startup;

var pluginLoader = new PluginLoader();
pluginLoader.Initialize();

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.AddMoonlight(pluginLoader.Instances);

var app = builder.Build();

app.ConfigureMoonlight(pluginLoader.Instances);

await app.RunAsync();