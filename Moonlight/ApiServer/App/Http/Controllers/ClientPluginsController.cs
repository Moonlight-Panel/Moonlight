using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;

namespace Moonlight.ApiServer.App.Http.Controllers;

// This is the controller for the plugin loading of the client

[Route("clientPlugins")]
[ApiController]
public class ClientPluginsController : Controller
{
    [HttpGet]
    public async Task<ActionResult<string[]>> GetAll()
    {
        var fileNames = Directory
            .EnumerateFiles(PathBuilder.Dir("storage", "clientPlugins"))
            .Select(x => Path.GetFileName(x))
            .ToArray();

        return Ok(fileNames);
    }

    [HttpGet("stream")]
    public async Task<ActionResult> Get([FromQuery] string name)
    {
        if (name.Contains(".."))
            return NotFound();

        var path = PathBuilder.File("storage", "clientPlugins", name);

        if (!System.IO.File.Exists(path))
            return NotFound();

        return File(System.IO.File.OpenRead(path), "application/octet-stream", name);
    }
}