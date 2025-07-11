using Moonlight.ApiServer;
using Moonlight.ApiServer.Runtime;

var startup = new Startup();

var pluginLoader = new PluginLoader();
pluginLoader.Initialize();

await startup.Run(args, pluginLoader.Instances);