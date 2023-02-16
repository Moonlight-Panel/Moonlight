using Logging.Net;
using Microsoft.AspNetCore.Mvc;

namespace Moonlight.App.Http.Controllers.Api.Moonlight;

[ApiController]
[Route("api/moonlight/resources")]
public class Resources : Controller
{
    [HttpGet("images/{name}")]
    public ActionResult GetImage([FromRoute] string name)
    {
        if (name.Contains(".."))
        {
            //TODO: Add security warn
            return NotFound();
        }

        var fs = new FileStream($"resources/public/images/{name}", FileMode.Open);
        
        return File(fs, "application/octet-stream", name);
    }
}