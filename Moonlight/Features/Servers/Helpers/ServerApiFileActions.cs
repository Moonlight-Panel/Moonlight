using MoonCore.Helpers;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.Servers.Exceptions;

namespace Moonlight.Features.Servers.Helpers;

public class ServerApiFileActions : IFileActions, IArchiveFileActions
{
    private readonly string Endpoint;
    private readonly string Token;
    private readonly int ServerId;

    private readonly HttpApiClient<NodeException> ApiClient;

    public ServerApiFileActions(string endpoint, string token, int serverId)
    {
        Endpoint = endpoint;
        Token = token;
        ServerId = serverId;

        ApiClient = new(endpoint + $"files/{ServerId}/", token);
    }

    public async Task<FileEntry[]> List(string path) => await ApiClient.Get<FileEntry[]>($"list?path={path}");

    public async Task DeleteFile(string path) => await ApiClient.DeleteAsString($"deleteFile?path={path}");

    public async Task DeleteDirectory(string path) => await ApiClient.DeleteAsString($"deleteDirectory?path={path}");

    public async Task Move(string from, string to) => await ApiClient.Post($"move?from={from}&to={to}");

    public async Task CreateDirectory(string path) => await ApiClient.Post($"createDirectory?path={path}");

    public async Task CreateFile(string path) => await ApiClient.Post($"createFile?path={path}");

    public async Task<string> ReadFile(string path) => await ApiClient.GetAsString($"readFile?path={path}");

    public async Task WriteFile(string path, string content) =>
        await ApiClient.PostAsString($"writeFile?path={path}", content);

    public async Task<Stream> ReadFileStream(string path) => await ApiClient.GetAsStream($"readFileStream?path={path}");

    public async Task WriteFileStream(string path, Stream dataStream) =>
        await ApiClient.PostFile($"writeFileStream?path={path}", dataStream, "upload");

    public async Task Archive(string path, string[] files)
    {
        await ApiClient.Post($"archive?path={path}&provider=tar.gz", files);
    }

    public async Task Extract(string path, string destination)
    {
        await ApiClient.Post($"extract?path={path}&destination={destination}&provider=tar.gz");
    }
    
    public IFileActions Clone() => new ServerApiFileActions(Endpoint, Token, ServerId);

    public void Dispose() => ApiClient.Dispose();
}