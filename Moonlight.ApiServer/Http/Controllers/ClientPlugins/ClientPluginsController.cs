using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Responses.ClientPlugins;

namespace Moonlight.ApiServer.Http.Controllers.ClientPlugins;

[ApiController]
[Route("api/clientPlugins")]
public class ClientPluginsController : Controller
{
    private readonly ModuleService ModuleService;

    public ClientPluginsController(ModuleService moduleService)
    {
        ModuleService = moduleService;
    }

    [HttpGet]
    public async Task<ClientPluginsResponse> Get()
    {
        var dlls = ModuleService.Modules
            .Where(x => x.Modules.ContainsKey("client"))
            .SelectMany(x =>
                x.Modules
                    .FirstOrDefault(c => c.Key == "client")
                    .Value
                    .Select(y => x.Name + "." + y)
            )
            .ToArray();

        return new()
        {
            CacheKey = "unset",
            Dlls = dlls
        };
    }
}