using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.App.Attributes;
using Moonlight.Shared.Models;

namespace Moonlight.ApiServer.App.Http.Controllers.Admin.Sys;

[ApiController]
[Route("admin/system/config")]
public class ConfigController : Controller
{
    private readonly ConfigService<AppConfiguration> ConfigService;

    public ConfigController(ConfigService<AppConfiguration> configService)
    {
        ConfigService = configService;
    }

    [HttpGet]
    [RequirePermission("admin.system.config.load")]
    public Task<ActionResult<AppConfiguration>> Load()
    {
        return Task.FromResult<ActionResult<AppConfiguration>>(Ok(ConfigService.Get()));
    }
    
    [HttpPost]
    [RequirePermission("admin.system.config.save")]
    public async Task<ActionResult> Save([FromBody] AppConfiguration configuration)
    {
        // Write new config to disk
        var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions()
        {
            WriteIndented = true
        });

        await System.IO.File.WriteAllTextAsync(
            PathBuilder.File("storage", "config.json"),
            json
        );
        
        ConfigService.Reload();

        return NoContent();
    }
}