using System.Text;
using Logging.Net;
using Microsoft.AspNetCore.Mvc;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Moonlight.App.Services;
using Moonlight.App.Services.Files;
using Moonlight.App.Services.LogServices;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Http.Controllers.Api.Moonlight;

[ApiController]
[Route("api/moonlight/resources")]
public class ResourcesController : Controller
{
    private readonly SecurityLogService SecurityLogService;
    private readonly BucketService BucketService;
    private readonly BundleService BundleService;

    public ResourcesController(SecurityLogService securityLogService,
        BucketService bucketService, BundleService bundleService)
    {
        SecurityLogService = securityLogService;
        BucketService = bucketService;
        BundleService = bundleService;
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

    [HttpGet("bundle/js")]
    public Task<ActionResult> GetJs()
    {
        if (BundleService.BundledFinished)
        {
            return Task.FromResult<ActionResult>(
                File(Encoding.ASCII.GetBytes(BundleService.BundledJs), "text/javascript")
            );
        }

        return Task.FromResult<ActionResult>(
            NotFound()
        );
    }
    
    [HttpGet("bundle/css")]
    public Task<ActionResult> GetCss()
    {
        if (BundleService.BundledFinished)
        {
            return Task.FromResult<ActionResult>(
                File(Encoding.ASCII.GetBytes(BundleService.BundledCss), "text/css")
            );
        }

        return Task.FromResult<ActionResult>(
            NotFound()
        );
    }
}