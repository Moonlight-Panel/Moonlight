using Logging.Net;
using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Moonlight.App.Services;
using Moonlight.App.Services.LogServices;

namespace Moonlight.App.Http.Controllers.Api.Moonlight;

[ApiController]
[Route("api/moonlight/resources")]
public class ResourcesController : Controller
{
    private readonly SecurityLogService SecurityLogService;
    private readonly BucketService BucketService;

    public ResourcesController(SecurityLogService securityLogService,
        BucketService bucketService)
    {
        SecurityLogService = securityLogService;
        BucketService = bucketService;
    }

    [HttpGet("images/{name}")]
    public async Task<ActionResult> GetImage([FromRoute] string name)
    {
        if (name.Contains(".."))
        {
            await SecurityLogService.Log(SecurityLogType.PathTransversal, x =>
            {
                x.Add<string>(name);
            });
            
            return NotFound();
        }

        if (System.IO.File.Exists(PathBuilder.File("storage", "resources", "public", "images", name)))
        {
            var fs = new FileStream(PathBuilder.File("storage", "resources", "public", "images", name), FileMode.Open);
        
            return File(fs, MimeTypes.GetMimeType(name), name);
        }

        return NotFound();
    }

    [HttpGet("bucket/{bucket}/{name}")]
    public async Task<ActionResult> GetBucket([FromRoute] string bucket, [FromRoute] string name)
    {
        if (name.Contains(".."))
        {
            await SecurityLogService.Log(SecurityLogType.PathTransversal, x =>
            {
                x.Add<string>(name);
            });
            
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