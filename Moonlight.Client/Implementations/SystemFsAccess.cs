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

    public async Task CreateFile(string path)
    {
        await ApiClient.Post(
            $"{BaseApiUrl}/touch?path={path}"
        );
    }

    public async Task CreateDirectory(string path)
    {
        await ApiClient.Post(
            $"{BaseApiUrl}/mkdir?path={path}"
        );
    }

    public async Task<FsEntry[]> List(string path)
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

    public async Task Move(string oldPath, string newPath)
    {
        await ApiClient.Post(
            $"{BaseApiUrl}/move?oldPath={oldPath}&newPath={newPath}"
        );
    }

    public async Task Read(string path, Func<Stream, Task> onHandleData)
    {
        await using var stream = await ApiClient.GetStream(
            $"{BaseApiUrl}/download?path={path}"
        );

        await onHandleData.Invoke(stream);

        stream.Close();
    }

    public async Task Write(string path, Stream dataStream)
    {
        using var multiPartForm = new MultipartFormDataContent();

        multiPartForm.Add(new StreamContent(dataStream), "file", "file");

        await ApiClient.Post(
            $"{BaseApiUrl}/upload?path={path}",
            multiPartForm
        );
    }

    public async Task Delete(string path)
    {
        await ApiClient.Delete(
            $"{BaseApiUrl}/delete?path={path}"
        );
    }

    public async Task Combine(string destination, string[] files)
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

    public ArchiveFormat[] ArchiveFormats { get; } = new[]
    {
        new ArchiveFormat()
        {
            DisplayName = "Zip Archive",
            Extensions = ["zip"],
            Identifier = "zip"
        },
        new ArchiveFormat()
        {
            DisplayName = "Tar.gz Archive",
            Extensions = ["tar.gz"],
            Identifier = "tar.gz"
        }
    };

    public async Task Archive(
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

    public async Task Unarchive(
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

    public async Task<string> GetFileUrl(string path)
        => await GetDownloadUrl(path);

    public async Task<string> GetFolderUrl(string path)
        => await GetDownloadUrl(path);

    private async Task<string> GetDownloadUrl(string path)
    {
        var response = await ApiClient.PostJson<DownloadUrlResponse>(
            $"{BaseApiUrl}/downloadUrl?path={path}"
        );

        return response.Url;
    }
}