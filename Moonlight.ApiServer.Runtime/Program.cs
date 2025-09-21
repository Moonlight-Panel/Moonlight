using Moonlight.ApiServer.Runtime;
using Moonlight.ApiServer.Startup;

var pluginLoader = new PluginLoader();
pluginLoader.Initialize();
/*
await startup.Run(args, pluginLoader.Instances);

*/

var cs = new Startup();

await cs.InitializeAsync(args, pluginLoader.Instances);

var builder = WebApplication.CreateBuilder(args);

await cs.AddMoonlightAsync(builder);

var app = builder.Build();

await cs.AddMoonlightAsync(app);

// Handle setup of wasm app hosting in the runtime
// so the Moonlight.ApiServer doesn't need the wasm package
if (cs.Configuration.Frontend.EnableHosting)
{
    if (app.Environment.IsDevelopment())
        app.UseWebAssemblyDebugging();

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
}


await app.RunAsync();