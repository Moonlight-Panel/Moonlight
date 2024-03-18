using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;
using Moonlight.Core.Services;

namespace Moonlight.Core.Http.Controllers.Api;

[ApiController]
[Route("api/bucket")]
public class BucketController : Controller
{
    private readonly BucketService BucketService;

    public BucketController(BucketService bucketService)
    {
        BucketService = bucketService;
    }

    [HttpGet("{bucket}/{file}")]
    public async Task<ActionResult> Get([FromRoute] string bucket, [FromRoute] string file) // TODO: Implement auth
    {
        if (bucket.Contains("..") || file.Contains(".."))
        {
            Logger.Warn($"Detected path transversal attack ({Request.HttpContext.Connection.RemoteIpAddress}).", "security");
            return NotFound();
        }

        try
        {
            var stream = await BucketService.Pull(bucket, file);
            return File(stream, MimeTypes.GetMimeType(file));
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }
}