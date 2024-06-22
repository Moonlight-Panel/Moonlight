using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Services;
using Moonlight.Features.FileManager.Models.Enums;
using Moonlight.Features.FileManager.Services;

namespace Moonlight.Features.FileManager.Http.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : Controller
{
    private readonly JwtService<FileManagerJwtType> JwtService;
    private readonly SharedFileAccessService SharedFileAccessService;
    private readonly ILogger<UploadController> Logger;
    
    public UploadController(
        JwtService<FileManagerJwtType> jwtService,
        SharedFileAccessService sharedFileAccessService, ILogger<UploadController> logger)
    {
        JwtService = jwtService;
        SharedFileAccessService = sharedFileAccessService;
        Logger = logger;
    }
    
    // The following method/api endpoint needs some explanation:
    // Because of blazor and dropzone.js, we need an api endpoint
    // to upload files via the built in file manager.
    // As we learned from user experiences in v1b
    // a large data transfer via the signal r connection might lead to
    // failing uploads for some users with a unstable connection. That's
    // why we implement this api endpoint. It can potentially prevent
    // upload from malicious scripts as well. To verify the user is
    // authenticated we use a jwt.
    // The jwt specifies what
    // connection we want to upload the file. This jwt
    // will be generated every 5 minutes in the file upload service
    // and only last 6 minutes.


    [HttpPost]
    public async Task<ActionResult> Upload([FromQuery(Name = "token")] string uploadToken)
    {
        // Check if a file exist and if it is not too big
        if (!Request.Form.Files.Any())
            return BadRequest("File is missing in request");

        if (Request.Form.Files.Count > 1)
            return BadRequest("Too many files sent");

        if (!Request.Form.ContainsKey("path"))
            return BadRequest("Path is missing");

        var path = Request.Form["path"].First() ?? "";

        if (string.IsNullOrEmpty(path))
            return BadRequest("Path is missing");
        
        if (path.Contains(".."))
        {
            Logger.LogWarning("A path transversal attack has been detected while processing upload path: {path}", path);
            return BadRequest("Invalid path. This attempt has been logged ;)");
        }

        // Validate request
        if (!await JwtService.Validate(uploadToken, FileManagerJwtType.FileAccess))
            return StatusCode(403);

        var uploadContext = await JwtService.Decode(uploadToken);

        if (!uploadContext.ContainsKey("FileAccessId"))
            return BadRequest();

        if (!int.TryParse(uploadContext["FileAccessId"], out int fileAccessId))
            return BadRequest();

        // Load file access for this file
        var fileAccess = await SharedFileAccessService.Get(fileAccessId);

        if (fileAccess == null)
            return BadRequest("Invalid file access id");
        
        // Actually upload the file
        var file = Request.Form.Files.First();
        await fileAccess.WriteFileStream(path, file.OpenReadStream());
        
        // Cleanup
        fileAccess.Dispose();

        return Ok();
    }
}