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
    private readonly FrontendService FrontendService;
    private readonly PluginService PluginService;

    public FrontendController(FrontendService frontendService, PluginService pluginService)
    {
        FrontendService = frontendService;
        PluginService = pluginService;
    }

    [HttpGet("frontend.json")]
    public async Task<FrontendConfiguration> GetConfiguration()
        => await FrontendService.GetConfiguration();

    [HttpGet("plugins/{assemblyName}")]
    public async Task GetPluginAssembly(string assemblyName)
    {
        var assembliesMap = PluginService.GetAssemblies("client");

        if (!assembliesMap.TryGetValue(assemblyName, out var path))
            throw new HttpApiException("The requested assembly could not be found", 404);

        var absolutePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            path
        );

        await Results.File(absolutePath).ExecuteAsync(HttpContext);
    }
}