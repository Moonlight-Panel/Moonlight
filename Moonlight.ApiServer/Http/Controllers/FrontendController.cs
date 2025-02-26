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
    private readonly AssetService AssetService;

    public FrontendController(
        AppConfiguration configuration,
        PluginService pluginService,
        AssetService assetService
    )
    {
        Configuration = configuration;
        PluginService = pluginService;
        AssetService = assetService;
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
        
        // Load theme if it exists
        var themePath = PathBuilder.File("storage", "theme.json");

        if (System.IO.File.Exists(themePath))
        {
            var variablesJson = await System.IO.File.ReadAllTextAsync(themePath);
            configuration.Theme.Variables = JsonSerializer.Deserialize<Dictionary<string, string>>(variablesJson) ?? new();
        }

        configuration.Plugins.Entrypoints = PluginService.HostedPluginsManifest.Entrypoints;
        configuration.Plugins.Assemblies = PluginService.HostedPluginsManifest.Assemblies;

        configuration.Scripts = AssetService.GetJavascriptAssets();

        return configuration;
    }

    [HttpGet("plugins/{assemblyName}")] // TODO: Test this
    public async Task GetPluginAssembly(string assemblyName)
    {
        var assembliesMap = PluginService.ClientAssemblyMap;

        if (assembliesMap.ContainsKey(assemblyName))
            throw new HttpApiException("The requested assembly could not be found", 404);

        var path = assembliesMap[assemblyName];

        await Results.File(path).ExecuteAsync(HttpContext);
    }
}