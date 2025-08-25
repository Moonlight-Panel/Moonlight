using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Helpers;
using Moonlight.Shared.Http.Responses.Admin.Sys;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys.Files;

[ApiController]
[Route("api/admin/system/files/downloadUrl")]
[Authorize(Policy = "permissions:admin.system.files")]
public class DownloadUrlController : Controller
{
    private readonly AppConfiguration Configuration;

    private const string BaseDirectory = "storage";

    public DownloadUrlController(AppConfiguration configuration)
    {
        Configuration = configuration;
    }

    [HttpGet]
    public async Task Get([FromQuery] string path)
    {
        var physicalPath = Path.Combine(BaseDirectory, FilePathHelper.SanitizePath(path));
        var name = Path.GetFileName(physicalPath);

        if (System.IO.File.Exists(physicalPath))
        {
            await using var fs = System.IO.File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            await Results.File(fs, fileDownloadName: name).ExecuteAsync(HttpContext);
        }
        else if(Directory.Exists(physicalPath))
        {
            // Without the base directory we would have the full path to the target folder
            // inside the zip
            
            var baseDirectory = Path.Combine(
                BaseDirectory,
                FilePathHelper.SanitizePath(Path.GetDirectoryName(path) ?? "")
            );
            
            Response.StatusCode = 200;
            Response.ContentType = "application/zip";
            Response.Headers["Content-Disposition"] = $"attachment; filename=\"{name}.zip\"";

            try
            {
                await using var zipStream = new ZipOutputStream(Response.Body);
                zipStream.IsStreamOwner = false;

                await StreamFolderAsZip(zipStream, physicalPath, baseDirectory, HttpContext.RequestAborted);
            }
            catch (ZipException)
            {
                // Ignored
            }
            catch (TaskCanceledException)
            {
                // Ignored
            }
        }
    }

    private async Task StreamFolderAsZip(
        ZipOutputStream zipStream,
        string path, string rootPath,
        CancellationToken cancellationToken
    )
    {
        foreach (var file in Directory.EnumerateFiles(path))
        {
            if (HttpContext.RequestAborted.IsCancellationRequested)
                return;

            var fi = new FileInfo(file);

            var filePath = Formatter.ReplaceStart(file, rootPath, "");

            await zipStream.PutNextEntryAsync(new ZipEntry(filePath)
            {
                Size = fi.Length,
            }, cancellationToken);

            await using var fs = System.IO.File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            await fs.CopyToAsync(zipStream, cancellationToken);
            await fs.FlushAsync(cancellationToken);

            fs.Close();

            await zipStream.FlushAsync(cancellationToken);
        }

        foreach (var directory in Directory.EnumerateDirectories(path))
        {
            if (HttpContext.RequestAborted.IsCancellationRequested)
                return;

            await StreamFolderAsZip(zipStream, directory, rootPath, cancellationToken);
        }
    }


    // Yes I know we can just create that url on the client as the exist validation is done on both endpoints,
    // but we leave it here for future modifications. E.g. using a distributed file provider or smth like that
    [HttpPost]
    public Task<DownloadUrlResponse> Post([FromQuery] string path)
    {
        var safePath = FilePathHelper.SanitizePath(path);
        var physicalPath = Path.Combine(BaseDirectory, safePath);

        if (System.IO.File.Exists(physicalPath) || Directory.Exists(physicalPath))
        {
            return Task.FromResult(new DownloadUrlResponse()
            {
                Url = $"{Configuration.PublicUrl}/api/admin/system/files/downloadUrl?path={path}"
            });
        }

        throw new HttpApiException("No such file or directory found", 404);
    }
}