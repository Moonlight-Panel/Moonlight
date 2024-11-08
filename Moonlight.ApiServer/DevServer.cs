namespace Moonlight.ApiServer;

public static class DevServer
{
    public async static Task Run(string[] args)
    {
        Console.WriteLine("Preparing development server");
        await Program.Main(args);
    }
}