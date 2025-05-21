using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.Shared.Http.Requests.Admin.Sys.Files;
using Moonlight.Shared.Http.Responses.Admin.Sys;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys;

[ApiController]
[Route("api/admin/system/files")]
[Authorize(Policy = "permissions:admin.system.files")]
public class FilesController : Controller
{
    private readonly string BaseDirectory = PathBuilder.Dir("storage");
    private readonly long ChunkSize = ByteConverter.FromMegaBytes(20).Bytes;

    [HttpGet("list")]
    public Task<FileSystemEntryResponse[]> List([FromQuery] string path)
    {
        var safePath = SanitizePath(path);
        var physicalPath = PathBuilder.Dir(BaseDirectory, safePath);

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
                IsFile = true,
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
                IsFile = false
            });
        }

        return Task.FromResult(
            entries.ToArray()
        );
    }

    [HttpPost("upload")]
    public async Task Upload([FromQuery] string path, [FromQuery] long totalSize, [FromQuery] int chunkId)
    {
        if (Request.Form.Files.Count != 1)
            throw new HttpApiException("You need to provide exactly one file", 400);

        var file = Request.Form.Files[0];
        
        if (file.Length > ChunkSize)
            throw new HttpApiException("The provided data exceeds the chunk size limit", 400);
        
        var chunks = totalSize / ChunkSize;
        chunks += totalSize % ChunkSize > 0 ? 1 : 0;

        if (chunkId > chunks)
            throw new HttpApiException("Invalid chunk id: Out of bounds", 400);
        
        var positionToSkipTo = ChunkSize * chunkId;

        var safePath = SanitizePath(path);
        var physicalPath = PathBuilder.File(BaseDirectory, safePath);
        var baseDir = Path.GetDirectoryName(physicalPath);

        if (!string.IsNullOrEmpty(baseDir))
            Directory.CreateDirectory(baseDir);

        await using var fs = System.IO.File.Open(physicalPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

        // This resizes the file to the correct size so we can handle the chunk if it didnt exist
        
        if (fs.Length != totalSize)
            fs.SetLength(totalSize);
        
        fs.Position = positionToSkipTo;

        var dataStream = file.OpenReadStream();

        await dataStream.CopyToAsync(fs);
        await fs.FlushAsync();

        fs.Close();
    }

    [HttpPost("move")]
    public Task Move([FromQuery] string oldPath, [FromQuery] string newPath)
    {
        var oldSafePath = SanitizePath(oldPath);
        var newSafePath = SanitizePath(newPath);

        var oldPhysicalDirPath = PathBuilder.Dir(BaseDirectory, oldSafePath);

        if (Directory.Exists(oldPhysicalDirPath))
        {
            var newPhysicalDirPath = PathBuilder.Dir(BaseDirectory, newSafePath);

            Directory.Move(
                oldPhysicalDirPath,
                newPhysicalDirPath
            );
        }
        else
        {
            var oldPhysicalFilePath = PathBuilder.File(BaseDirectory, oldSafePath);
            var newPhysicalFilePath = PathBuilder.File(BaseDirectory, newSafePath);

            System.IO.File.Move(
                oldPhysicalFilePath,
                newPhysicalFilePath
            );
        }

        return Task.CompletedTask;
    }

    [HttpDelete("delete")]
    public Task Delete([FromQuery] string path)
    {
        var safePath = SanitizePath(path);
        var physicalDirPath = PathBuilder.Dir(BaseDirectory, safePath);

        if (Directory.Exists(physicalDirPath))
            Directory.Delete(physicalDirPath, true);
        else
        {
            var physicalFilePath = PathBuilder.File(BaseDirectory, safePath);

            System.IO.File.Delete(physicalFilePath);
        }

        return Task.CompletedTask;
    }

    [HttpPost("mkdir")]
    public Task CreateDirectory([FromQuery] string path)
    {
        var safePath = SanitizePath(path);
        var physicalPath = PathBuilder.Dir(BaseDirectory, safePath);

        Directory.CreateDirectory(physicalPath);
        return Task.CompletedTask;
    }

    [HttpGet("download")]
    public async Task Download([FromQuery] string path)
    {
        var safePath = SanitizePath(path);
        var physicalPath = PathBuilder.File(BaseDirectory, safePath);

        await using var fs = System.IO.File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        await fs.CopyToAsync(Response.Body);
        
        fs.Close();
    }

    [HttpPost("compress")]
    public async Task Compress([FromBody] CompressRequest request)
    {
        if (request.Type == "tar.gz")
            await CompressTarGz(request.Path, request.ItemsToCompress);
        else if (request.Type == "zip")
            await CompressZip(request.Path, request.ItemsToCompress);
    }

    #region Tar Gz

    private async Task CompressTarGz(string path, string[] itemsToCompress)
    {
        var safePath = SanitizePath(path);
        var destination = PathBuilder.File(BaseDirectory, safePath);

        await using var outStream = System.IO.File.Create(destination);
        await using var gzoStream = new GZipOutputStream(outStream);
        await using var tarStream = new TarOutputStream(gzoStream, Encoding.UTF8);

        foreach (var itemName in itemsToCompress)
        {
            var safeFilePath = SanitizePath(itemName);
            var filePath = PathBuilder.File(BaseDirectory, safeFilePath);

            var fi = new FileInfo(filePath);

            if (fi.Exists)
                await AddFileToTarGz(tarStream, filePath);
            else
            {
                var safeDirePath = SanitizePath(itemName);
                var dirPath = PathBuilder.Dir(BaseDirectory, safeDirePath);

                await AddDirectoryToTarGz(tarStream, dirPath);
            }
        }

        await tarStream.FlushAsync();
        await gzoStream.FlushAsync();
        await outStream.FlushAsync();

        tarStream.Close();
        gzoStream.Close();
        outStream.Close();
    }

    private async Task AddDirectoryToTarGz(TarOutputStream tarOutputStream, string root)
    {
        foreach (var file in Directory.GetFiles(root))
            await AddFileToTarGz(tarOutputStream, file);

        foreach (var directory in Directory.GetDirectories(root))
            await AddDirectoryToTarGz(tarOutputStream, directory);
    }

    private async Task AddFileToTarGz(TarOutputStream tarOutputStream, string file)
    {
        // Open file stream
        var fs = System.IO.File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        // Meta 
        var entry = TarEntry.CreateTarEntry(file);

        // Fix path
        entry.Name = Formatter
            .ReplaceStart(entry.Name, BaseDirectory, "")
            .TrimStart('/');

        entry.Size = fs.Length;

        // Write entry
        await tarOutputStream.PutNextEntryAsync(entry, CancellationToken.None);

        // Copy file content to tar stream
        await fs.CopyToAsync(tarOutputStream);
        fs.Close();

        // Close the entry
        tarOutputStream.CloseEntry();
    }

    #endregion

    #region ZIP

    private async Task CompressZip(string path, string[] itemsToCompress)
    {
        var safePath = SanitizePath(path);
        var destination = PathBuilder.File(BaseDirectory, safePath);

        await using var outStream = System.IO.File.Create(destination);
        await using var zipOutputStream = new ZipOutputStream(outStream);

        foreach (var itemName in itemsToCompress)
        {
            var safeFilePath = SanitizePath(itemName);
            var filePath = PathBuilder.File(BaseDirectory, safeFilePath);

            var fi = new FileInfo(filePath);

            if (fi.Exists)
                await AddFileToZip(zipOutputStream, filePath);
            else
            {
                var safeDirePath = SanitizePath(itemName);
                var dirPath = PathBuilder.Dir(BaseDirectory, safeDirePath);

                await AddDirectoryToZip(zipOutputStream, dirPath);
            }
        }

        await zipOutputStream.FlushAsync();
        await outStream.FlushAsync();

        zipOutputStream.Close();
        outStream.Close();
    }

    private async Task AddFileToZip(ZipOutputStream zipOutputStream, string path)
    {
        // Open file stream
        var fs = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        // Fix path
        var name = Formatter
            .ReplaceStart(path, BaseDirectory, "")
            .TrimStart('/');

        // Meta 
        var entry = new ZipEntry(name);

        entry.Size = fs.Length;

        // Write entry
        await zipOutputStream.PutNextEntryAsync(entry, CancellationToken.None);

        // Copy file content to tar stream
        await fs.CopyToAsync(zipOutputStream);
        fs.Close();

        // Close the entry
        zipOutputStream.CloseEntry();
    }

    private async Task AddDirectoryToZip(ZipOutputStream zipOutputStream, string root)
    {
        foreach (var file in Directory.GetFiles(root))
            await AddFileToZip(zipOutputStream, file);

        foreach (var directory in Directory.GetDirectories(root))
            await AddDirectoryToZip(zipOutputStream, directory);
    }

    #endregion

    [HttpPost("decompress")]
    public async Task Decompress([FromBody] DecompressRequest request)
    {
        if (request.Type == "tar.gz")
            await DecompressTarGz(request.Path, request.Destination);
        else if (request.Type == "zip")
            await DecompressZip(request.Path, request.Destination);
    }

    #region Tar Gz

    private async Task DecompressTarGz(string path, string destination)
    {
        var safeDestination = SanitizePath(destination);

        var safeArchivePath = SanitizePath(path);
        var archivePath = PathBuilder.File(BaseDirectory, safeArchivePath);

        await using var fs = System.IO.File.Open(archivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        await using var gzipInputStream = new GZipInputStream(fs);
        await using var tarInputStream = new TarInputStream(gzipInputStream, Encoding.UTF8);

        while (true)
        {
            var entry = await tarInputStream.GetNextEntryAsync(CancellationToken.None);

            if (entry == null)
                break;

            var safeFilePath = SanitizePath(entry.Name);
            var fileDestination = PathBuilder.File(BaseDirectory, safeDestination, safeFilePath);
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
        var safeDestination = SanitizePath(destination);

        var safeArchivePath = SanitizePath(path);
        var archivePath = PathBuilder.File(BaseDirectory, safeArchivePath);

        await using var fs = System.IO.File.Open(archivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        await using var zipInputStream = new ZipInputStream(fs);

        while (true)
        {
            var entry = zipInputStream.GetNextEntry();

            if (entry == null)
                break;

            if (entry.IsDirectory)
                continue;

            var safeFilePath = SanitizePath(entry.Name);
            var fileDestination = PathBuilder.File(BaseDirectory, safeDestination, safeFilePath);
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

    private string SanitizePath(string path)
        => path.Replace("..", "");
}