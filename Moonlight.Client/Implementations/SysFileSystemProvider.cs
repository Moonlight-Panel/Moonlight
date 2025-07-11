using MoonCore.Blazor.Services;
using MoonCore.Blazor.Tailwind.Fm;
using MoonCore.Blazor.Tailwind.Fm.Models;
using MoonCore.Helpers;
using Moonlight.Shared.Http.Requests.Admin.Sys.Files;
using Moonlight.Shared.Http.Responses.Admin.Sys;

namespace Moonlight.Client.Implementations;

public class SysFileSystemProvider : IFileSystemProvider, ICompressFileSystemProvider
{
    private readonly DownloadService DownloadService;
    private readonly HttpApiClient HttpApiClient;
    private readonly LocalStorageService LocalStorageService;
    private readonly string BaseApiUrl = "api/admin/system/files";

    public CompressType[] CompressTypes { get; } =
    [
        new()
        {
            Extension = "zip",
            DisplayName = "ZIP Archive"
        },
        new()
        {
            Extension = "tar.gz",
            DisplayName = "GZ Compressed Tar Archive"
        }
    ];

    public SysFileSystemProvider(
        HttpApiClient httpApiClient,
        DownloadService downloadService,
        LocalStorageService localStorageService
    )
    {
        HttpApiClient = httpApiClient;
        DownloadService = downloadService;
        LocalStorageService = localStorageService;
    }

    public async Task<FileSystemEntry[]> List(string path)
    {
        var entries = await HttpApiClient.GetJson<FileSystemEntryResponse[]>(
            $"{BaseApiUrl}/list?path={path}"
        );

        return entries.Select(x => new FileSystemEntry()
        {
            Name = x.Name,
            Size = x.Size,
            CreatedAt = x.CreatedAt,
            IsFile = x.IsFile,
            UpdatedAt = x.UpdatedAt
        }).ToArray();
    }

    public async Task Create(string path, Stream stream)
    {
        await Upload(_ => Task.CompletedTask, path, stream);
    }

    public async Task Move(string oldPath, string newPath)
        => await HttpApiClient.Post($"{BaseApiUrl}/move?oldPath={oldPath}&newPath={newPath}");

    public async Task Delete(string path)
        => await HttpApiClient.Delete($"{BaseApiUrl}/delete?path={path}");

    public async Task CreateDirectory(string path)
        => await HttpApiClient.Post($"{BaseApiUrl}/mkdir?path={path}");

    public async Task<Stream> Read(string path)
        => await HttpApiClient.GetStream($"{BaseApiUrl}/download?path={path}");

    public async Task Download(Func<int, Task> updateProgress, string path, string fileName)
    {
        var accessToken = await LocalStorageService.GetString("AccessToken");

        await DownloadService.DownloadUrl(fileName, $"{BaseApiUrl}/download?path={path}",
            async (loaded, total) =>
            {
                var percent = total == 0 ? 0 : (int)Math.Round((float)loaded / total * 100);
                await updateProgress.Invoke(percent);
            },
            onConfigureHeaders: headers => { headers.Add("Authorization", $"Bearer {accessToken}"); }
        );
    }

    public async Task Upload(Func<int, Task> updateProgress, string path, Stream stream)
    {
        var size = stream.Length;
        var chunkSize = ByteConverter.FromMegaBytes(20).Bytes;

        var chunks = size / chunkSize;
        chunks += size % chunkSize > 0 ? 1 : 0;
        
        for (var chunkId = 0; chunkId < chunks; chunkId++)
        {
            var percent = (int)Math.Round((chunkId + 1f) / chunks * 100);
            await updateProgress.Invoke(percent);
            
            var buffer = new byte[chunkSize];
            var bytesRead = await stream.ReadAsync(buffer);

            var uploadForm = new MultipartFormDataContent();
            uploadForm.Add(new ByteArrayContent(buffer, 0, bytesRead), "file", path);

            await HttpApiClient.Post(
                $"{BaseApiUrl}/upload?path={path}&totalSize={size}&chunkId={chunkId}",
                uploadForm
            );
        }
    }

    public async Task Compress(CompressType type, string path, string[] itemsToCompress)
    {
        await HttpApiClient.Post($"{BaseApiUrl}/compress", new CompressRequest()
        {
            Type = type.Extension,
            Path = path,
            ItemsToCompress = itemsToCompress
        });
    }

    public async Task Decompress(CompressType type, string path, string destination)
    {
        await HttpApiClient.Post($"{BaseApiUrl}/decompress", new DecompressRequest()
        {
            Type = type.Extension,
            Path = path,
            Destination = destination
        });
    }
}