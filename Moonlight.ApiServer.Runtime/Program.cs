using Moonlight.ApiServer;
using Moonlight.ApiServer.Runtime;

var pluginLoader = new PluginLoader();
pluginLoader.Initialize();
/*
await startup.Run(args, pluginLoader.Instances);

*/

var cs = new CleanStartup();

var builder = WebApplication.CreateBuilder();

await cs.AddMoonlight(builder, args, pluginLoader.Instances);

var app = builder.Build();

await cs.AddMoonlight(app, args, pluginLoader.Instances);

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();


await app.RunAsync();