using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using MoonCore.Models;
using Moonlight.ApiServer.Services;

namespace Moonlight.ApiServer.Http.Controllers.Assets;

[ApiController]
[Route("api/assets/plugins")]
public class AssetsPluginsController : Controller
{
    private readonly PluginService PluginService;

    public AssetsPluginsController(PluginService pluginService)
    {
        PluginService = pluginService;
    }

    [HttpGet]
    public Task<HostedPluginsManifest> GetManifest()
    {
        return Task.FromResult(PluginService.HostedPluginsManifest);
    }

    [HttpGet("stream")]
    public async Task GetAssembly([FromQuery(Name = "assembly")] string assembly)
    {
        var assembliesMap = PluginService.ClientAssemblyMap;

        if (assembliesMap.ContainsKey(assembly))
            throw new HttpApiException("The requested assembly could not be found", 404);

        var path = assembliesMap[assembly];

        await Results.File(path).ExecuteAsync(HttpContext);
    }
}