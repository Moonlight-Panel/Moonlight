using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;
using Moonlight.Core.Services.Utils;
using Moonlight.Features.FileManager.Services;

namespace Moonlight.Features.FileManager.Http.Controllers;

[ApiController]
[Route("api/download")]
public class DownloadController : Controller
{
    private readonly JwtService JwtService;
    private readonly SharedFileAccessService SharedFileAccessService;

    public DownloadController(JwtService jwtService, SharedFileAccessService sharedFileAccessService)
    {
        JwtService = jwtService;
        SharedFileAccessService = sharedFileAccessService;
    }
    
    [HttpGet]
    public async Task<ActionResult> Upload([FromQuery(Name = "token")] string downloadToken, [FromQuery(Name = "name")] string name)
    {
        if (name.Contains(".."))
        {
            Logger.Warn($"A user tried to access a file via path transversal. Name: {name}");
            return NotFound();
        }
        
        // Validate request
        if (!await JwtService.Validate(downloadToken, "FileAccess"))
            return StatusCode(403);

        var downloadContext = await JwtService.Decode(downloadToken);

        if (!downloadContext.ContainsKey("FileAccessId"))
            return BadRequest();

        if (!int.TryParse(downloadContext["FileAccessId"], out int fileAccessId))
            return BadRequest();

        // Load file access for this file
        var fileAccess = await SharedFileAccessService.Get(fileAccessId);

        if (fileAccess == null)
            return BadRequest("Invalid file access id");

        var files = await fileAccess.List();

        if (files.All(x => !x.IsFile && x.Name != name))
            return NotFound();

        var stream = await fileAccess.ReadFileStream(name);

        return File(stream, "application/octet-stream", name);
    }
}