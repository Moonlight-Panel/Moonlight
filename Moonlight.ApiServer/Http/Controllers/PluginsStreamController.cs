using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MoonCore.Exceptions;
using MoonCore.Models;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Responses.PluginsStream;

namespace Moonlight.ApiServer.Http.Controllers;

[ApiController]
[Route("api/pluginsStream")]
public class PluginsStreamController : Controller
{
    private readonly PluginService PluginService;
    private readonly IMemoryCache Cache;

    public PluginsStreamController(PluginService pluginService, IMemoryCache cache)
    {
        PluginService = pluginService;
        Cache = cache;
    }

    [HttpGet]
    public Task<HostedPluginsManifest> GetManifest()
    {
        return Task.FromResult(PluginService.HostedPluginsManifest);
    }

    [HttpGet("stream")]
    public async Task GetAssembly([FromQuery(Name = "assembly")] string assembly)
    {
        var assembliesMap = PluginService.AssemblyMap;

        if (assembliesMap.ContainsKey(assembly))
            throw new HttpApiException("The requested assembly could not be found", 404);

        var path = assembliesMap[assembly];

        await Results.File(path).ExecuteAsync(HttpContext);
    }

    [HttpGet("assets")]
    public Task<PluginsAssetManifest> GetAssetManifest()
    {
        return Task.FromResult(PluginService.PluginsAssetManifest);
    }
}