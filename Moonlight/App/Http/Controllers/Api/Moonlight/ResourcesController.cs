using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Helpers;
using Moonlight.App.Services.Files;

namespace Moonlight.App.Http.Controllers.Api.Moonlight;

[ApiController]
[Route("api/moonlight/resources")]
public class ResourcesController : Controller
{
    private readonly BucketService BucketService;

    public ResourcesController(BucketService bucketService)
    {
        BucketService = bucketService;
    }

    [HttpGet("images/{name}")]
    public async Task<ActionResult> GetImage([FromRoute] string name)
    {
        if (name.Contains(".."))
        {
            Logger.Warn($"Detected an attempted path transversal. Path: {name}", "security");
            
            return NotFound();
        }

        if (System.IO.File.Exists(PathBuilder.File("storage", "resources", "public", "images", name)))
        {
            var fs = new FileStream(PathBuilder.File("storage", "resources", "public", "images", name), FileMode.Open);
        
            return File(fs, MimeTypes.GetMimeType(name), name);
        }

        return NotFound();
    }
    
    [HttpGet("background/{name}")]
    public async Task<ActionResult> GetBackground([FromRoute] string name)
    {
        if (name.Contains(".."))
        {
            Logger.Warn($"Detected an attempted path transversal. Path: {name}", "security");
            
            return NotFound();
        }

        if (System.IO.File.Exists(PathBuilder.File("storage", "resources", "public", "background", name)))
        {
            var fs = new FileStream(PathBuilder.File("storage", "resources", "public", "background", name), FileMode.Open);
        
            return File(fs, MimeTypes.GetMimeType(name), name);
        }

        return NotFound();
    }

    [HttpGet("bucket/{bucket}/{name}")]
    public async Task<ActionResult> GetBucket([FromRoute] string bucket, [FromRoute] string name)
    {
        if (name.Contains(".."))
        {
            Logger.Warn($"Detected an attempted path transversal. Path: {name}", "security");
            
            return NotFound();
        }

        try
        {
            var fs = await BucketService.GetFile(bucket, name);
            
            return File(fs, MimeTypes.GetMimeType(name), name);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            return Problem();
        }
    }
}