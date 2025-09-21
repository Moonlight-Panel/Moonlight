using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Moonlight.Shared.Misc;

namespace Moonlight.Client.Startup;

public partial class Startup
{
    private Task PrintVersionAsync()
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

        return Task.CompletedTask;
    }

    private async Task LoadConfigurationAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(WebAssemblyHostBuilder.HostEnvironment.BaseAddress);

            var jsonText = await httpClient.GetStringAsync("frontend.json");

            Configuration = JsonSerializer.Deserialize<FrontendConfiguration>(jsonText, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            })!;

            WebAssemblyHostBuilder.Services.AddSingleton(Configuration);
        }
        catch (Exception e)
        {
            Logger.LogCritical("Unable to load configuration. Unable to continue: {e}", e);
            throw;
        }
    }
}