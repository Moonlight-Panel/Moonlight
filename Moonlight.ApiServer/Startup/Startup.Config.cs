using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoonCore.EnvConfiguration;
using MoonCore.Yaml;
using Moonlight.ApiServer.Configuration;

namespace Moonlight.ApiServer.Startup;

public partial class Startup
{
    private async Task SetupAppConfigurationAsync()
    {
        var configPath = Path.Combine("storage", "config.yml");

        await YamlDefaultGenerator.GenerateAsync<AppConfiguration>(configPath);

        // Configure configuration (wow)
        var configurationBuilder = new ConfigurationBuilder();

        configurationBuilder.AddYamlFile(configPath);
        configurationBuilder.AddEnvironmentVariables(prefix: "MOONLIGHT_", separator: "_");

        var configurationRoot = configurationBuilder.Build();
        
        // Retrieve configuration
        Configuration = AppConfiguration.CreateEmpty();
        configurationRoot.Bind(Configuration);
    }

    private Task RegisterAppConfigurationAsync()
    {
        WebApplicationBuilder.Services.AddSingleton(Configuration);
        return Task.CompletedTask;
    }
}