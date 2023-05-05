using System.Web;
using Moonlight.App.ApiClients.Wings;
using Moonlight.App.ApiClients.Wings.Requests;
using Moonlight.App.ApiClients.Wings.Resources;
using Moonlight.App.Database.Entities;
using Moonlight.App.Services;
using RestSharp;

namespace Moonlight.App.Helpers.Files;

public class WingsFileAccess : FileAccess
{
    private readonly WingsApiHelper WingsApiHelper;
    private readonly WingsJwtHelper WingsJwtHelper;
    private readonly ConfigService ConfigService;
    private readonly Server Server;
    private readonly User User;

    public WingsFileAccess(
        WingsApiHelper wingsApiHelper, 
        WingsJwtHelper wingsJwtHelper, 
        Server server,
        ConfigService configService, 
        User user)
    {
        WingsApiHelper = wingsApiHelper;
        WingsJwtHelper = wingsJwtHelper;
        Server = server;
        ConfigService = configService;
        User = user;

        if (server.Node == null)
        {
            throw new ArgumentException("The wings file access server model needs to include the node data");
        }
    }

    public override async Task<FileData[]> Ls()
    {
        var res = await WingsApiHelper.Get<ListDirectory[]>(
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

    public override Task Cd(string dir)
    {
        var x = Path.Combine(CurrentPath, dir).Replace("\\", "/") + "/";
        x = x.Replace("//", "/");
        CurrentPath = x;

        return Task.CompletedTask;
    }

    public override Task Up()
    {
        CurrentPath = Path.GetFullPath(Path.Combine(CurrentPath, "..")).Replace("\\", "/").Replace("C:", "");
        return Task.CompletedTask;
    }

    public override Task SetDir(string dir)
    {
        CurrentPath = dir;
        return Task.CompletedTask;
    }

    public override async Task<string> Read(FileData fileData)
    {
        return await WingsApiHelper.GetRaw(Server.Node,
            $"api/servers/{Server.Uuid}/files/contents?file={CurrentPath}{fileData.Name}");
    }

    public override async Task Write(FileData fileData, string content)
    {
        await WingsApiHelper.PostRaw(Server.Node,
            $"api/servers/{Server.Uuid}/files/write?file={CurrentPath}{fileData.Name}", content);
    }

    public override async Task Upload(string name, Stream dataStream, Action<int>? progressUpdated = null)
    {
        var token = WingsJwtHelper.Generate(
            Server.Node.Token,
            claims => { claims.Add("server_uuid", Server.Uuid.ToString()); }
        );

        var client = new RestClient();
        var request = new RestRequest();

        if (Server.Node.Ssl)
            request.Resource =
                $"https://{Server.Node.Fqdn}:{Server.Node.HttpPort}/upload/file?token={token}&directory={CurrentPath}";
        else
            request.Resource =
                $"http://{Server.Node.Fqdn}:{Server.Node.HttpPort}/upload/file?token={token}&directory={CurrentPath}";

        request.AddParameter("name", "files");
        request.AddParameter("filename", name);
        request.AddHeader("Content-Type", "multipart/form-data");
        request.AddHeader("Origin", ConfigService.GetSection("Moonlight").GetValue<string>("AppUrl"));
        request.AddFile("files", () =>
        {
            return new StreamProgressHelper(dataStream)
            {
                Progress = i => { progressUpdated?.Invoke(i); }
            };
        }, name);

        await client.ExecutePostAsync(request);

        client.Dispose();
        dataStream.Close();
    }

    public override async Task MkDir(string name)
    {
        await WingsApiHelper.Post(Server.Node, $"api/servers/{Server.Uuid}/files/create-directory",
            new CreateDirectory()
            {
                Name = name,
                Path = CurrentPath
            }
        );
    }

    public override Task<string> Pwd()
    {
        return Task.FromResult(CurrentPath);
    }

    public override Task<string> DownloadUrl(FileData fileData)
    {
        var token = WingsJwtHelper.Generate(Server.Node.Token, claims =>
        {
            claims.Add("server_uuid", Server.Uuid.ToString());
            claims.Add("file_path", CurrentPath + "/" + fileData.Name);
        });

        if (Server.Node.Ssl)
        {
            return Task.FromResult(
                $"https://{Server.Node.Fqdn}:{Server.Node.HttpPort}/download/file?token={token}"
            );
        }
        else
        {
            return Task.FromResult(
                $"http://{Server.Node.Fqdn}:{Server.Node.HttpPort}/download/file?token={token}"
            );
        }
    }

    public override Task<Stream> DownloadStream(FileData fileData)
    {
        throw new NotImplementedException();
    }

    public override async Task Delete(FileData fileData)
    {
        await WingsApiHelper.Post(Server.Node, $"api/servers/{Server.Uuid}/files/delete", new DeleteFiles()
        {
            Root = CurrentPath,
            Files = new()
            {
                fileData.Name
            }
        });
    }

    public override async Task Move(FileData fileData, string newPath)
    {
        var req = new RenameFiles()
        {
            Root = "/",
            Files = new[]
            {
                new RenameFilesData()
                {
                    From = (CurrentPath + fileData.Name),
                    To = newPath
                }
            }
        };
        
        await WingsApiHelper.Put(Server.Node, $"api/servers/{Server.Uuid}/files/rename", req);
    }

    public override async Task Compress(params FileData[] files)
    {
        var req = new CompressFiles()
        {
            Root = CurrentPath,
            Files = files.Select(x => x.Name).ToArray()
        };

        await WingsApiHelper.Post(Server.Node, $"api/servers/{Server.Uuid}/files/compress", req);
    }

    public override async Task Decompress(FileData fileData)
    {
        var req = new DecompressFile()
        {
            Root = CurrentPath,
            File = fileData.Name
        };

        await WingsApiHelper.Post(Server.Node, $"api/servers/{Server.Uuid}/files/decompress", req);
    }

    public override Task<string> GetLaunchUrl()
    {
        return Task.FromResult(
                    $"sftp://{User.Id}.{StringHelper.IntToStringWithLeadingZeros(Server.Id, 8)}@{Server.Node.Fqdn}:{Server.Node.SftpPort}");
    }

    public override object Clone()
    {
        return new WingsFileAccess(WingsApiHelper, WingsJwtHelper, Server, ConfigService, User);
    }
}