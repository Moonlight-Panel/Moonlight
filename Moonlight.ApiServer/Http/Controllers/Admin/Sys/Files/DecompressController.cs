using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moonlight.ApiServer.Helpers;
using Moonlight.Shared.Http.Requests.Admin.Sys.Files;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys.Files;

[ApiController]
[Route("api/admin/system/files")]
[Authorize(Policy = "permissions:admin.system.files")]
public class DecompressController : Controller
{
    private const string BaseDirectory = "storage";
    
    [HttpPost("decompress")]
    public async Task Decompress([FromBody] DecompressRequest request)
    {
        var path = Path.Combine(BaseDirectory, FilePathHelper.SanitizePath(request.Path));
        var destination = Path.Combine(BaseDirectory, FilePathHelper.SanitizePath(request.Destination));
        
        switch (request.Format)
        {
            case "tar.gz":
                await DecompressTarGz(path, destination);
                break;
            
            case "zip":
                await DecompressZip(path, destination);
                break;
        }
    }
    
     #region Tar Gz

    private async Task DecompressTarGz(string path, string destination)
    {
        await using var fs = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        await using var gzipInputStream = new GZipInputStream(fs);
        await using var tarInputStream = new TarInputStream(gzipInputStream, Encoding.UTF8);

        while (true)
        {
            var entry = await tarInputStream.GetNextEntryAsync(CancellationToken.None);

            if (entry == null)
                break;

            var safeFilePath = FilePathHelper.SanitizePath(entry.Name);
            var fileDestination = Path.Combine(destination, safeFilePath);
            var parentFolder = Path.GetDirectoryName(fileDestination);

            // Ensure parent directory exists, if it's not the base directory
            if (parentFolder != null && parentFolder != BaseDirectory)
                Directory.CreateDirectory(parentFolder);

            await using var fileDestinationFs =
                System.IO.File.Open(fileDestination, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            await tarInputStream.CopyToAsync(fileDestinationFs, CancellationToken.None);

            await fileDestinationFs.FlushAsync();
            fileDestinationFs.Close();
        }

        tarInputStream.Close();
        gzipInputStream.Close();
        fs.Close();
    }

    #endregion

    #region Zip

    private async Task DecompressZip(string path, string destination)
    {
        await using var fs = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        await using var zipInputStream = new ZipInputStream(fs);

        while (true)
        {
            var entry = zipInputStream.GetNextEntry();

            if (entry == null)
                break;

            if (entry.IsDirectory)
                continue;

            var safeFilePath = FilePathHelper.SanitizePath(entry.Name);
            var fileDestination = Path.Combine(destination, safeFilePath);
            var parentFolder = Path.GetDirectoryName(fileDestination);

            // Ensure parent directory exists, if it's not the base directory
            if (parentFolder != null && parentFolder != BaseDirectory)
                Directory.CreateDirectory(parentFolder);

            await using var fileDestinationFs =
                System.IO.File.Open(fileDestination, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            await zipInputStream.CopyToAsync(fileDestinationFs, CancellationToken.None);

            await fileDestinationFs.FlushAsync();
            fileDestinationFs.Close();
        }

        zipInputStream.Close();
        fs.Close();
    }

    #endregion
}