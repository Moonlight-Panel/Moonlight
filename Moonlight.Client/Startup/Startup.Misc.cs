namespace Moonlight.Client.Startup;

public static partial class Startup
{
    private static void PrintVersion()
    {
        // Fancy start console output... yes very fancy :>
        Console.Write("Running ");

        var rainbow = new Crayon.Rainbow(0.5);
        foreach (var c in "Moonlight")
        {
            Console.Write(
                rainbow
                    .Next()
                    .Bold()
                    .Text(c.ToString())
            );
        }

        Console.WriteLine();
    }
}