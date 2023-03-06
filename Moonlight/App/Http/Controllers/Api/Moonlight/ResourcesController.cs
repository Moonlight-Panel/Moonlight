using Logging.Net;
using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Models.Misc;
using Moonlight.App.Services.LogServices;

namespace Moonlight.App.Http.Controllers.Api.Moonlight;

[ApiController]
[Route("api/moonlight/resources")]
public class ResourcesController : Controller
{
    private readonly SecurityLogService SecurityLogService;

    public ResourcesController(SecurityLogService securityLogService)
    {
        SecurityLogService = securityLogService;
    }

    [HttpGet("images/{name}")]
    public async Task<ActionResult> GetImage([FromRoute] string name)
    {
        if (name.Contains(".."))
        {
            await SecurityLogService.Log(SecurityLogType.PathTransversal, name);
            return NotFound();
        }

        if (System.IO.File.Exists($"resources/public/images/{name}"))
        {
            var fs = new FileStream($"resources/public/images/{name}", FileMode.Open);
        
            return File(fs, MimeTypes.GetMimeType(name), name);
        }
        
        Logger.Debug("404 on resources");
        return NotFound();
    }
}