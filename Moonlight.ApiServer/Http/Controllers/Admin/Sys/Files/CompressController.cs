using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;
using Moonlight.ApiServer.Helpers;
using Moonlight.Shared.Http.Requests.Admin.Sys.Files;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys.Files;

[ApiController]
[Route("api/admin/system/files")]
[Authorize(Policy = "permissions:admin.system.files")]
public class CompressController : Controller
{
    private const string BaseDirectory = "storage";

    [HttpPost("compress")]
    public async Task<IResult> Compress([FromBody] CompressRequest request)
    {
        // Validate item length
        if (request.Items.Length == 0)
        {
            return Results.Problem(
                "At least one item is required",
                statusCode: 400
            );
        }

        // Build paths
        var destinationPath = Path.Combine(BaseDirectory, FilePathHelper.SanitizePath(request.Destination));
        var rootPath = Path.Combine(BaseDirectory, FilePathHelper.SanitizePath(request.Root));

        // Resolve the relative to the root item paths to absolute paths
        var itemsPaths = request.Items.Select(item =>
            Path.Combine(
                BaseDirectory,
                FilePathHelper.SanitizePath(
                    UnixPath.Combine(request.Root, item)
                )
            )
        );

        // Execute request
        switch (request.Format)
        {
            case "tar.gz":
                await CompressTarGz(destinationPath, itemsPaths, rootPath);
                break;

            case "zip":
                await CompressZip(destinationPath, itemsPaths, rootPath);
                break;

            default:
                return Results.Problem("Unsupported archive format specified", statusCode: 400);
        }

        return Results.Ok();
    }

    

    #region Tar Gz

    private async Task CompressTarGz(string destination, IEnumerable<string> items, string root)
    {
        await using var outStream = System.IO.File.Create(destination);
        await using var gzoStream = new GZipOutputStream(outStream);
        await using var tarStream = new TarOutputStream(gzoStream, Encoding.UTF8);

        foreach (var item in items)
            await CompressItemToTarGz(tarStream, item, root);

        await tarStream.FlushAsync();
        await gzoStream.FlushAsync();
        await outStream.FlushAsync();

        tarStream.Close();
        gzoStream.Close();
        outStream.Close();
    }

    private async Task CompressItemToTarGz(TarOutputStream tarOutputStream, string item, string root)
    {
        if (System.IO.File.Exists(item))
        {
            // Open file stream
            var fs = System.IO.File.Open(item, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Meta 
            var entry = TarEntry.CreateTarEntry(
                Formatter
                    .ReplaceStart(item, root, "")
                    .TrimStart('/')
            );

            // Set size
            entry.Size = fs.Length;

            // Write entry
            await tarOutputStream.PutNextEntryAsync(entry, CancellationToken.None);

            // Copy file content to tar stream
            await fs.CopyToAsync(tarOutputStream);
            fs.Close();

            // Close the entry
            tarOutputStream.CloseEntry();

            return;
        }

        if (Directory.Exists(item))
        {
            foreach (var fsEntry in Directory.EnumerateFileSystemEntries(item))
                await CompressItemToTarGz(tarOutputStream, fsEntry, root);
        }
    }

    #endregion

    #region ZIP

    private async Task CompressZip(string destination, IEnumerable<string> items, string root)
    {
        await using var outStream = System.IO.File.Create(destination);
        await using var zipOutputStream = new ZipOutputStream(outStream);

        foreach (var item in items)
            await AddItemToZip(zipOutputStream, item, root);

        await zipOutputStream.FlushAsync();
        await outStream.FlushAsync();

        zipOutputStream.Close();
        outStream.Close();
    }

    private async Task AddItemToZip(ZipOutputStream outputStream, string item, string root)
    {
        if (System.IO.File.Exists(item))
        {
            // Open file stream
            var fs = System.IO.File.Open(item, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Meta 
            var entry = new ZipEntry(
                Formatter
                    .ReplaceStart(item, root, "")
                    .TrimStart('/')
            );

            entry.Size = fs.Length;

            // Write entry
            await outputStream.PutNextEntryAsync(entry, CancellationToken.None);

            // Copy file content to tar stream
            await fs.CopyToAsync(outputStream);
            fs.Close();

            // Close the entry
            outputStream.CloseEntry();

            // Flush caches
            await outputStream.FlushAsync();

            return;
        }

        if (Directory.Exists(item))
        {
            foreach (var subItem in Directory.EnumerateFileSystemEntries(item))
                await AddItemToZip(outputStream, subItem, root);
        }
    }

    #endregion
}