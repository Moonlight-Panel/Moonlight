using MoonCore.Blazor.FlyonUi.Files;
using MoonCore.Blazor.FlyonUi.Files.Manager;
using MoonCore.Helpers;
using Moonlight.Shared.Http.Responses.Admin.Sys;

namespace Moonlight.Client.Implementations;

public class SystemFsAccess : IFsAccess
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

    public Task Read(string path, Func<Stream, Task> onHandleData)
    {
        throw new NotImplementedException();
    }

    public Task Write(string path, Stream dataStream)
    {
        throw new NotImplementedException();
    }

    public async Task Delete(string path)
    {
        await ApiClient.Delete(
            $"{BaseApiUrl}/delete?path={path}"
        );
    }

    public async Task UploadChunk(string path, int chunkId, long chunkSize, long totalSize, byte[] data)
    {
        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(data), "file", "file");
        
        await ApiClient.Post(
            $"{BaseApiUrl}/upload?path={path}&chunkId={chunkId}&chunkSize={chunkSize}&totalSize={totalSize}",
            formContent
        );
    }

    public Task<byte[]> DownloadChunk(string path, int chunkId, long chunkSize)
    {
        throw new NotImplementedException();
    }
}