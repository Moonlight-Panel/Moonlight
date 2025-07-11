using Moonlight.Client;
using Moonlight.Client.Runtime;

var startup = new Startup();

var pluginLoader = new PluginLoader();
pluginLoader.Initialize();

await startup.Run(args, pluginLoader.Instances);