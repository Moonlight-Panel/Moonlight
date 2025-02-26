using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Misc;

namespace Moonlight.ApiServer.Http.Controllers;

[ApiController]
public class FrontendController : Controller
{
    private readonly AppConfiguration Configuration;
    private readonly PluginService PluginService;

    public FrontendController(
        AppConfiguration configuration,
        PluginService pluginService
    )
    {
        Configuration = configuration;
        PluginService = pluginService;
    }

    [HttpGet("frontend.json")]
    public async Task<FrontendConfiguration> GetConfiguration()
    {
        var configuration = new FrontendConfiguration()
        {
            Title = "Moonlight",
            ApiUrl = Configuration.PublicUrl,
            HostEnvironment = "ApiServer"
        };

        #region Load theme.json if it exists

        var themePath = PathBuilder.File("storage", "theme.json");

        if (System.IO.File.Exists(themePath))
        {
            var variablesJson = await System.IO.File.ReadAllTextAsync(themePath);
            configuration.Theme.Variables =
                JsonSerializer.Deserialize<Dictionary<string, string>>(variablesJson) ?? new();
        }

        #endregion

        // Collect assemblies for the 'client' section
        configuration.Assemblies = PluginService
            .GetAssemblies("client")
            .Keys
            .ToArray();

        // Collect scripts to execute
        configuration.Scripts = PluginService
            .LoadedPlugins
            .Keys
            .SelectMany(x => x.Scripts)
            .ToArray();

        // Collect styles
        var styles = new List<string>();

        styles.AddRange(
            PluginService
                .LoadedPlugins
                .Keys
                .SelectMany(x => x.Styles)
        );

        // Add bundle css
        styles.Add("css/bundle.min.css");

        configuration.Styles = styles.ToArray();

        return configuration;
    }

    [HttpGet("plugins/{assemblyName}")]
    public async Task GetPluginAssembly(string assemblyName)
    {
        var assembliesMap = PluginService.GetAssemblies("client");

        if (assembliesMap.ContainsKey(assemblyName))
            throw new HttpApiException("The requested assembly could not be found", 404);

        var path = assembliesMap[assemblyName];

        await Results.File(path).ExecuteAsync(HttpContext);
    }
}