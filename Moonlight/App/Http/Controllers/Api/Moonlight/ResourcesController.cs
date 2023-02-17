using Logging.Net;
using Microsoft.AspNetCore.Mvc;

namespace Moonlight.App.Http.Controllers.Api.Moonlight;

[ApiController]
[Route("api/moonlight/resources")]
public class ResourcesController : Controller
{
    [HttpGet("images/{name}")]
    public ActionResult GetImage([FromRoute] string name)
    {
        if (name.Contains(".."))
        {
            //TODO: Add security warn
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