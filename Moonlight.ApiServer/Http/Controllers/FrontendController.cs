using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
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
    public Task<FrontendConfiguration> GetConfiguration()
    {
        var configuration = new FrontendConfiguration()
        {
            Title = "Moonlight",
            ApiUrl = Configuration.PublicUrl,
            HostEnvironment = "ApiServer"
        };

        configuration.Plugins.Entrypoints = PluginService.HostedPluginsManifest.Entrypoints;
        configuration.Plugins.Assemblies = PluginService.HostedPluginsManifest.Assemblies;

        configuration.Scripts = AssetService.GetJavascriptAssets();

        return Task.FromResult(configuration);
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