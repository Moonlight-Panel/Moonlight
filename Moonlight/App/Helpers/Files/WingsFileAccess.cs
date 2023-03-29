using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Wings.Resources;

namespace Moonlight.App.Helpers.Files;

public class WingsFileAccess : IFileAccess
{
    private readonly WingsApiHelper WingsApiHelper;
    private readonly WingsJwtHelper WingsJwtHelper;
    private readonly Server Server;

    private string CurrentPath = "/";

    public WingsFileAccess(WingsApiHelper wingsApiHelper, WingsJwtHelper wingsJwtHelper,Server server)
    {
        WingsApiHelper = wingsApiHelper;
        WingsJwtHelper = wingsJwtHelper;
        Server = server;

        if (server.Node == null)
        {
            throw new ArgumentException("The wings file access server model needs to include the node data");
        }
    }

    public async Task<FileData[]> Ls()
    {
        var res = await WingsApiHelper.Get<ListDirectoryRequest[]>(
            Server.Node,
            $"api/servers/{Server.Uuid}/files/list-directory?directory={CurrentPath}"
        );

        var x = new List<FileData>();

        foreach (var response in res)
        {
            x.Add(new()
            {
                Name = response.Name,
                Size = response.File ? response.Size : 0,
                IsFile = response.File,
            });
        }

        return x.ToArray();
    }

    public Task Cd(string dir)
    {
        var x = Path.Combine(CurrentPath, dir).Replace("\\", "/") + "/";
        x = x.Replace("//", "/");
        CurrentPath = x;

        return Task.CompletedTask;
    }

    public Task Up()
    {
        CurrentPath = Path.GetFullPath(Path.Combine(CurrentPath, "..")).Replace("\\", "/").Replace("C:", "");
        return Task.CompletedTask;
    }
    
    public Task SetDir(string dir)
    {
        CurrentPath = dir;
        return Task.CompletedTask;
    }

    public async Task<string> Read(FileData fileData)
    {
        return await WingsApiHelper.GetRaw(Server.Node,$"api/servers/{Server.Uuid}/files/contents?file={CurrentPath}{fileData.Name}");
    }

    public async Task Write(FileData fileData, string content)
    {
        await WingsApiHelper.PostRaw(Server.Node,$"api/servers/{Server.Uuid}/files/write?file={CurrentPath}{fileData.Name}", content);
    }

    public Task Upload(string name, Stream stream, Action<int>? progressUpdated = null)
    {
        throw new NotImplementedException();
    }

    public Task MkDir(string name)
    {
        throw new NotImplementedException();
    }

    public Task<string> Pwd()
    {
        return Task.FromResult(CurrentPath);
    }

    public Task<string> DownloadUrl(FileData fileData)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> DownloadStream(FileData fileData)
    {
        throw new NotImplementedException();
    }

    public Task Delete(FileData fileData)
    {
        throw new NotImplementedException();
    }

    public Task Move(FileData fileData, string newPath)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetLaunchUrl()
    {
        throw new NotImplementedException();
    }
}