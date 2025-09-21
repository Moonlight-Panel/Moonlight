using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using Moonlight.ApiServer.Helpers;
using Moonlight.Shared.Http.Responses.Admin.Sys;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys.Files;

[ApiController]
[Route("api/admin/system/files")]
[Authorize(Policy = "permissions:admin.system.files")]
public class FilesController : Controller
{
    private const string BaseDirectory = "storage";

    [HttpPost("touch")]
    public async Task CreateFileAsync([FromQuery] string path)
    {
        var safePath = FilePathHelper.SanitizePath(path);
        var physicalPath = Path.Combine(BaseDirectory, safePath);

        if (System.IO.File.Exists(physicalPath))
            throw new HttpApiException("A file already exists at that path", 400);

        if (Directory.Exists(path))
            throw new HttpApiException("A folder already exists at that path", 400);

        await using var fs = System.IO.File.Create(physicalPath);
        fs.Close();
    }
    
    [HttpPost("mkdir")]
    public Task CreateFolderAsync([FromQuery] string path)
    {
        var safePath = FilePathHelper.SanitizePath(path);
        var physicalPath = Path.Combine(BaseDirectory, safePath);

        if (Directory.Exists(path))
            throw new HttpApiException("A folder already exists at that path", 400);
        
        if (System.IO.File.Exists(physicalPath))
            throw new HttpApiException("A file already exists at that path", 400);

        Directory.CreateDirectory(physicalPath);
        return Task.CompletedTask;
    }
    
    [HttpGet("list")]
    public Task<FileSystemEntryResponse[]> ListAsync([FromQuery] string path)
    {
        var safePath = FilePathHelper.SanitizePath(path);
        var physicalPath = Path.Combine(BaseDirectory, safePath);

        var entries = new List<FileSystemEntryResponse>();

        var files = Directory.GetFiles(physicalPath);

        foreach (var file in files)
        {
            var fi = new FileInfo(file);

            entries.Add(new FileSystemEntryResponse()
            {
                Name = fi.Name,
                Size = fi.Length,
                CreatedAt = fi.CreationTimeUtc,
                IsFolder = false,
                UpdatedAt = fi.LastWriteTimeUtc
            });
        }

        var directories = Directory.GetDirectories(physicalPath);

        foreach (var directory in directories)
        {
            var di = new DirectoryInfo(directory);

            entries.Add(new FileSystemEntryResponse()
            {
                Name = di.Name,
                Size = 0,
                CreatedAt = di.CreationTimeUtc,
                UpdatedAt = di.LastWriteTimeUtc,
                IsFolder = true
            });
        }

        return Task.FromResult(
            entries.ToArray()
        );
    }

    [HttpPost("move")]
    public Task MoveAsync([FromQuery] string oldPath, [FromQuery] string newPath)
    {
        var oldSafePath = FilePathHelper.SanitizePath(oldPath);
        var newSafePath = FilePathHelper.SanitizePath(newPath);

        var oldPhysicalDirPath = Path.Combine(BaseDirectory, oldSafePath);

        if (Directory.Exists(oldPhysicalDirPath))
        {
            var newPhysicalDirPath = Path.Combine(BaseDirectory, newSafePath);

            Directory.Move(
                oldPhysicalDirPath,
                newPhysicalDirPath
            );
        }
        else
        {
            var oldPhysicalFilePath = Path.Combine(BaseDirectory, oldSafePath);
            var newPhysicalFilePath = Path.Combine(BaseDirectory, newSafePath);

            System.IO.File.Move(
                oldPhysicalFilePath,
                newPhysicalFilePath
            );
        }

        return Task.CompletedTask;
    }

    [HttpDelete("delete")]
    public Task DeleteAsync([FromQuery] string path)
    {
        var safePath = FilePathHelper.SanitizePath(path);
        var physicalDirPath = Path.Combine(BaseDirectory, safePath);

        if (Directory.Exists(physicalDirPath))
            Directory.Delete(physicalDirPath, true);
        else
        {
            var physicalFilePath = Path.Combine(BaseDirectory, safePath);

            System.IO.File.Delete(physicalFilePath);
        }

        return Task.CompletedTask;
    }

    [HttpPost("upload")]
    public async Task<IResult> UploadAsync([FromQuery] string path)
    {
        if (Request.Form.Files.Count != 1)
            return Results.Problem("Only one file is allowed in the request", statusCode: 400);

        var file = Request.Form.Files[0];
        
        var safePath = FilePathHelper.SanitizePath(path);
        var physicalPath = Path.Combine(BaseDirectory, safePath);

        // Create directory which the new file should be put into
        var baseDirectory = Path.GetDirectoryName(physicalPath);
        
        if(!string.IsNullOrEmpty(baseDirectory))
            Directory.CreateDirectory(baseDirectory);

        // Create file from provided form
        await using var dataStream = file.OpenReadStream();

        await using var targetStream = System.IO.File.Open(
            physicalPath,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.Read
        );

        // Copy the content to the newly created file
        await dataStream.CopyToAsync(targetStream);
        await targetStream.FlushAsync();
        
        // Close both streams
        targetStream.Close();
        dataStream.Close();

        return Results.Ok();
    }

    [HttpGet("download")]
    public async Task DownloadAsync([FromQuery] string path)
    {
        var safePath = FilePathHelper.SanitizePath(path);
        var physicalPath = Path.Combine(BaseDirectory, safePath);

        await using var fs = System.IO.File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        await fs.CopyToAsync(Response.Body);
        
        fs.Close();
    }
}