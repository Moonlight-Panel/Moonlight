using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Moonlight.Client.Runtime;
using Moonlight.Client.Startup;

var pluginLoader = new PluginLoader();
pluginLoader.Initialize();

var startup = new Startup();

await startup.Initialize(pluginLoader.Instances);

var wasmHostBuilder = WebAssemblyHostBuilder.CreateDefault(args);

await startup.AddMoonlight(wasmHostBuilder);

var wasmApp = wasmHostBuilder.Build();

await startup.AddMoonlight(wasmApp);

await wasmApp.RunAsync();