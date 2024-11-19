using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MoonCore.Exceptions;
using MoonCore.Models;
using Moonlight.ApiServer.Services;

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
        var assembliesMap = Cache.GetOrCreate("clientPluginAssemblies", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

            return PluginService.GetAssemblies("client");
        })!;

        var entrypoints = Cache.GetOrCreate("clientPluginEntrypoints", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

            return PluginService.GetEntrypoints("client");
        })!;

        return Task.FromResult(new HostedPluginsManifest()
        {
            Assemblies = assembliesMap.Keys.ToArray(),
            Entrypoints = entrypoints
        });
    }

    [HttpGet("stream")]
    public async Task GetAssembly([FromQuery(Name = "assembly")] string assembly)
    {
        var assembliesMap = Cache.GetOrCreate("clientPluginAssemblies", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

            return PluginService.GetAssemblies("client");
        })!;

        if (assembliesMap.ContainsKey(assembly))
            throw new HttpApiException("The requested assembly could not be found", 404);

        var path = assembliesMap[assembly];

        await Results.File(path).ExecuteAsync(HttpContext);
    }
}