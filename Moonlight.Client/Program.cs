using Moonlight.Client;

var startup = new Startup();

try
{
    await startup.Run(args);
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}