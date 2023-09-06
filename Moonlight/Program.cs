namespace Moonlight
{
    public class Program
    {
        private static readonly Startup Startup = new();
        
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Moonlight Panel");
            Console.WriteLine($"Copyright {DateTime.UtcNow.Year} moonlightpanel.xyz");
            Console.WriteLine();

            await Startup.Init(args);
            await Startup.Start();
        }
    }
}