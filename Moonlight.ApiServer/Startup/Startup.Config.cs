using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoonCore.EnvConfiguration;
using MoonCore.Yaml;
using Moonlight.ApiServer.Configuration;

namespace Moonlight.ApiServer.Startup;

public static partial class Startup
{
    private static void AddConfiguration(this WebApplicationBuilder builder)
    {
        // Yaml
        var yamlPath = Path.Combine("storage", "config.yml");

        YamlDefaultGenerator.GenerateAsync<AppConfiguration>(yamlPath).Wait();

        builder.Configuration.AddYamlFile(yamlPath);
        
        // Env
        builder.Configuration.AddEnvironmentVariables(prefix: "MOONLIGHT_", separator: "_");
        
        var configuration = AppConfiguration.CreateEmpty();
        builder.Configuration.Bind(configuration);
        
        builder.Services.AddSingleton(configuration);
    }
}