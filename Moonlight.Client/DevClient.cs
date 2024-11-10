using System.Reflection;

namespace Moonlight.Client;

public class DevClient
{
    public async static Task Run(string[] args, Assembly[] assemblies)
    {
        Console.WriteLine("Preparing development client");
        await Startup.Run(args, assemblies);
    }
}