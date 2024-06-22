using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;
using Moonlight.Core.Attributes;

namespace Moonlight.Core.Http.Controllers;

[ApiController]
[ApiDocument("internal")]
[Route("api/core/asset")]
public class AssetController : Controller
{
    private readonly ILogger<AssetController> Logger;

    public AssetController(ILogger<AssetController> logger)
    {
        Logger = logger;
    }

    [HttpGet("{name}/{*path}")]
    public async Task<ActionResult> Get(string name, string path)
    {
        // Check for path transversal attacks
        if (path.Contains("..") || name.Contains(".."))
        {
            Logger.LogWarning("{remoteIp} tried to use path transversal attack: {name}/{path}", HttpContext.Connection.RemoteIpAddress, name, path);
            return NotFound();
        }
        
        // Build a local file path out of the asset path
        var localPath = PathBuilder.File(path.Split("/"));

        // Build override paths
        var overrideAssetPath = PathBuilder.Dir("storage", "assetOverrides", name);
        var overridePath = PathBuilder.File(overrideAssetPath, localPath);

        // Check if override exists, if yes, we want to return the override
        if (System.IO.File.Exists(overridePath))
        {
            var overrideStream = System.IO.File.OpenRead(overridePath);
            return File(overrideStream, MimeTypes.GetMimeType(Path.GetFileName(localPath)));
        }

        // Build asset paths
        var assetPath = PathBuilder.Dir("Assets", name);
        var realPath = PathBuilder.File(assetPath, localPath);

        // Check if file exists, if not, we return 404
        if (!System.IO.File.Exists(realPath))
            return NotFound("Invalid asset name or path");

        // If it exists, return the file
        var assetStream = System.IO.File.OpenRead(realPath);
        return File(assetStream, MimeTypes.GetMimeType(Path.GetFileName(localPath)));
    }
}