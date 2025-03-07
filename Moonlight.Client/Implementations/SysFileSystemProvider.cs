using MoonCore.Blazor.Tailwind.Fm;
using MoonCore.Blazor.Tailwind.Fm.Models;
using MoonCore.Helpers;
using Moonlight.Shared.Http.Requests.Admin.Sys.Files;
using Moonlight.Shared.Http.Responses.Admin.Sys;

namespace Moonlight.Client.Implementations;

public class SysFileSystemProvider : IFileSystemProvider, ICompressFileSystemProvider
{
    private readonly HttpApiClient HttpApiClient;
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

    public SysFileSystemProvider(HttpApiClient httpApiClient)
    {
        HttpApiClient = httpApiClient;
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
        var content = new MultipartFormDataContent();
        
        content.Add(new StreamContent(stream), "file", "x");
        
        await HttpApiClient.Post($"{BaseApiUrl}/create?path={path}", content);
    }

    public async Task Move(string oldPath, string newPath)
        => await HttpApiClient.Post($"{BaseApiUrl}/move?oldPath={oldPath}&newPath={newPath}");

    public async Task Delete(string path)
        => await HttpApiClient.Delete($"{BaseApiUrl}/delete?path={path}");

    public async Task CreateDirectory(string path)
        => await HttpApiClient.Post($"{BaseApiUrl}/mkdir?path={path}");

    public async Task<Stream> Read(string path)
        => await HttpApiClient.GetStream($"{BaseApiUrl}/read?path={path}");

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