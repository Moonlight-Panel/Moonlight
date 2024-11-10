using System.Reflection;

namespace Moonlight.ApiServer;

public static class DevServer
{
    public async static Task Run(string[] args, Assembly[] pluginAssemblies)
    {
        Console.WriteLine("Preparing development server");
        await Startup.Run(args, pluginAssemblies);
    }
}