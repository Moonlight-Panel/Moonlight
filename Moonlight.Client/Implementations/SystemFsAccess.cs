using MoonCore.Blazor.FlyonUi.Files;
using MoonCore.Blazor.FlyonUi.Files.Manager;
using MoonCore.Blazor.FlyonUi.Files.Manager.Abstractions;
using MoonCore.Helpers;
using Moonlight.Shared.Http.Requests.Admin.Sys.Files;
using Moonlight.Shared.Http.Responses.Admin.Sys;

namespace Moonlight.Client.Implementations;

public class SystemFsAccess : IFsAccess, ICombineAccess, IArchiveAccess, IDownloadUrlAccess
{
    private readonly HttpApiClient ApiClient;

    private const string BaseApiUrl = "api/admin/system/files";

    public SystemFsAccess(HttpApiClient apiClient)
    {
        ApiClient = apiClient;
    }

    public async Task CreateFileAsync(string path)
    {
        await ApiClient.Post(
            $"{BaseApiUrl}/touch?path={path}"
        );
    }

    public async Task CreateDirectoryAsync(string path)
    {
        await ApiClient.Post(
            $"{BaseApiUrl}/mkdir?path={path}"
        );
    }

    public async Task<FsEntry[]> ListAsync(string path)
    {
        var entries = await ApiClient.GetJson<FileSystemEntryResponse[]>(
            $"{BaseApiUrl}/list?path={path}"
        );

        return entries.Select(x => new FsEntry()
        {
            Name = x.Name,
            CreatedAt = x.CreatedAt,
            IsFolder = x.IsFolder,
            Size = x.Size,
            UpdatedAt = x.UpdatedAt
        }).ToArray();
    }

    public async Task MoveAsync(string oldPath, string newPath)
    {
        await ApiClient.Post(
            $"{BaseApiUrl}/move?oldPath={oldPath}&newPath={newPath}"
        );
    }

    public async Task ReadAsync(string path, Func<Stream, Task> onHandleData)
    {
        await using var stream = await ApiClient.GetStream(
            $"{BaseApiUrl}/download?path={path}"
        );

        await onHandleData.Invoke(stream);

        stream.Close();
    }

    public async Task WriteAsync(string path, Stream dataStream)
    {
        using var multiPartForm = new MultipartFormDataContent();

        multiPartForm.Add(new StreamContent(dataStream), "file", "file");

        await ApiClient.Post(
            $"{BaseApiUrl}/upload?path={path}",
            multiPartForm
        );
    }

    public async Task DeleteAsync(string path)
    {
        await ApiClient.Delete(
            $"{BaseApiUrl}/delete?path={path}"
        );
    }

    public async Task CombineAsync(string destination, string[] files)
    {
        await ApiClient.Post(
            $"{BaseApiUrl}/combine",
            new CombineRequest()
            {
                Destination = destination,
                Files = files
            }
        );
    }

    public ArchiveFormat[] ArchiveFormats { get; } =
    [
        new("zip", ["zip"], "Zip Archive"),
        new("tar.gz", ["tar.gz"], "Tar.gz Archive")
    ];

    public async Task ArchiveAsync(
        string destination,
        ArchiveFormat format,
        string root,
        FsEntry[] files,
        Func<string, Task>? onProgress = null
    )
    {
        await ApiClient.Post($"{BaseApiUrl}/compress", new CompressRequest()
        {
            Destination = destination,
            Items = files.Select(x => x.Name).ToArray(),
            Root = root,
            Format = format.Identifier
        });
    }

    public async Task UnarchiveAsync(
        string path,
        ArchiveFormat format,
        string destination,
        Func<string, Task>? onProgress = null)
    {
        await ApiClient.Post(
            $"{BaseApiUrl}/decompress",
            new DecompressRequest()
            {
                Format = format.Identifier,
                Destination = destination,
                Path = path
            }
        );
    }

    public async Task<string> GetFileUrlAsync(string path)
        => await GetDownloadUrlAsync(path);

    public async Task<string> GetFolderUrlAsync(string path)
        => await GetDownloadUrlAsync(path);

    private async Task<string> GetDownloadUrlAsync(string path)
    {
        var response = await ApiClient.PostJson<DownloadUrlResponse>(
            $"{BaseApiUrl}/downloadUrl?path={path}"
        );

        return response.Url;
    }
}