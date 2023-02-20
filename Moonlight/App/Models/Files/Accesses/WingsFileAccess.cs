using System.Security.Cryptography;
using System.Text;
using System.Web;
using JWT.Algorithms;
using JWT.Builder;
using Moonlight.App.Database.Entities;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Wings.Requests;
using Moonlight.App.Models.Wings.Resources;
using RestSharp;

namespace Moonlight.App.Models.Files.Accesses;

public class WingsFileAccess : IFileAccess
{
    private readonly WingsApiHelper WingsApiHelper;
    private readonly WingsJwtHelper WingsJwtHelper;
    private readonly Database.Entities.Node Node;
    private readonly Server Server;
    private readonly User User;
    private readonly string AppUrl;

    private string Path { get; set; } = "/";

    public WingsFileAccess(
        WingsApiHelper wingsApiHelper,
        Server server,
        User user,
        WingsJwtHelper wingsJwtHelper,
        string appUrl)
    {
        WingsApiHelper = wingsApiHelper;
        Node = server.Node;
        Server = server;
        User = user;
        WingsJwtHelper = wingsJwtHelper;
        AppUrl = appUrl;
    }

    public async Task<FileManagerObject[]> GetDirectoryContent()
    {
        var res = await WingsApiHelper.Get<ListDirectoryRequest[]>(Node,
            $"api/servers/{Server.Uuid}/files/list-directory?directory={Path}");

        var x = new List<FileManagerObject>();

        foreach (var response in res)
        {
            x.Add(new()
            {
                Name = response.Name,
                Size = response.File ? response.Size : 0,
                CreatedAt = response.Created.Date,
                IsFile = response.File,
                UpdatedAt = response.Modified.Date
            });
        }

        return x.ToArray();
    }

    public Task ChangeDirectory(string s)
    {
        var x = System.IO.Path.Combine(Path, s).Replace("\\", "/") + "/";
        x = x.Replace("//", "/");
        Path = x;

        return Task.CompletedTask;
    }

    public Task SetDirectory(string s)
    {
        Path = s;
        return Task.CompletedTask;
    }

    public Task GoUp()
    {
        Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Path, "..")).Replace("\\", "/").Replace("C:", "");
        return Task.CompletedTask;
    }

    public async Task<string> ReadFile(FileManagerObject fileManagerObject)
    {
        return await WingsApiHelper.GetRaw(Node,
            $"api/servers/{Server.Uuid}/files/contents?file={Path}{fileManagerObject.Name}");
    }

    public async Task WriteFile(FileManagerObject fileManagerObject, string content)
    {
        await WingsApiHelper.PostRaw(Node,
            $"api/servers/{Server.Uuid}/files/write?file={Path}{fileManagerObject.Name}", content);
    }

    public async Task UploadFile(string name, Stream dataStream, Action<int> onProgress)
    {
        var token = WingsJwtHelper.Generate(Node.Token,
            claims => { claims.Add("server_uuid", Server.Uuid.ToString()); });

        var client = new RestClient();
        var request = new RestRequest();

        if (Node.Ssl)
            request.Resource = $"https://{Node.Fqdn}:{Node.HttpPort}/upload/file?token={token}&directory={Path}";
        else
            request.Resource = $"http://{Node.Fqdn}:{Node.HttpPort}/upload/file?token={token}&directory={Path}";

        request.AddParameter("name", "files");
        request.AddParameter("filename", name);
        request.AddHeader("Content-Type", "multipart/form-data");
        request.AddHeader("Origin", AppUrl);
        request.AddFile("files", () =>
        {
            return new StreamProgressHelper(dataStream)
            {
                Progress = i =>
                {
                    if (onProgress != null)
                        onProgress.Invoke(i);
                }
            };
        }, name);

        await client.ExecutePostAsync(request);

        client.Dispose();
        dataStream.Close();
    }

    public async Task CreateDirectory(string name)
    {
        await WingsApiHelper.Post(Node, $"api/servers/{Server.Uuid}/files/create-directory",
            new CreateDirectoryRequest()
            {
                Name = name,
                Path = Path
            });
    }

    public Task<string> GetCurrentPath()
    {
        return Task.FromResult(Path);
    }

    public Task<Stream> GetDownloadStream(FileManagerObject managerObject)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetDownloadUrl(FileManagerObject managerObject)
    {
        var token = WingsJwtHelper.Generate(Node.Token, claims =>
        {
            claims.Add("server_uuid", Server.Uuid.ToString());
            claims.Add("file_path", HttpUtility.UrlDecode(Path + "/" + managerObject.Name));
        });

        if (Node.Ssl)
        {
            return Task.FromResult(
                $"https://{Node.Fqdn}:{Node.HttpPort}/download/file?token={token}"
            );
        }
        else
        {
            return Task.FromResult(
                $"http://{Node.Fqdn}:{Node.HttpPort}/download/file?token={token}"
            );
        }
    }

    public async Task Delete(FileManagerObject managerObject)
    {
        await WingsApiHelper.Post(Node, $"api/servers/{Server.Uuid}/files/delete", new DeleteFilesRequest()
        {
            Root = Path,
            Files = new()
            {
                managerObject.Name
            }
        });
    }

    public async Task Move(FileManagerObject managerObject, string newPath)
    {
        await WingsApiHelper.Put(Node, $"api/servers/{Server.Uuid}/files/rename", new RenameFilesRequest()
        {
            Root = "/",
            Files = new[]
            {
                new RenameFilesData()
                {
                    From = Path + managerObject.Name,
                    To = newPath
                }
            }
        });
    }

    public Task<string> GetLaunchUrl()
    {
        return Task.FromResult(
            $"sftp://{User.Id}.{StringHelper.IntToStringWithLeadingZeros(Server.Id, 8)}@{Node.Fqdn}:{Node.SftpPort}");
    }
}