using Logging.Net;
using Renci.SshNet;
using ConnectionInfo = Renci.SshNet.ConnectionInfo;

namespace Moonlight.App.Helpers.Files;

public class SftpFileAccess : FileAccess
{
    private readonly string SftpHost;
    private readonly string SftpUser;
    private readonly string SftpPassword;
    private readonly int SftpPort;
    private readonly bool ForceUserDir;

    private readonly SftpClient Client;

    private string InternalPath
    {
        get
        {
            if (ForceUserDir)
                return $"/home/{SftpUser}{CurrentPath}";

            return InternalPath;
        }
    }

    public SftpFileAccess(string sftpHost, string sftpUser, string sftpPassword, int sftpPort,
        bool forceUserDir = false)
    {
        SftpHost = sftpHost;
        SftpUser = sftpUser;
        SftpPassword = sftpPassword;
        SftpPort = sftpPort;
        ForceUserDir = forceUserDir;

        Client = new(
            new ConnectionInfo(
                SftpHost,
                SftpPort,
                SftpUser,
                new PasswordAuthenticationMethod(
                    SftpUser,
                    SftpPassword
                )
            )
        );
    }

    private void EnsureConnect()
    {
        if (!Client.IsConnected)
            Client.Connect();
    }


    public override Task<FileData[]> Ls()
    {
        EnsureConnect();

        var x = new List<FileData>();

        foreach (var file in Client.ListDirectory(InternalPath))
        {
            if (file.Name != "." && file.Name != "..")
            {
                x.Add(new()
                {
                    Name = file.Name,
                    Size = file.Attributes.Size,
                    IsFile = !file.IsDirectory
                });
            }
        }

        return Task.FromResult(x.ToArray());
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

    public override Task<string> Read(FileData fileData)
    {
        EnsureConnect();

        var textStream = Client.Open(InternalPath.TrimEnd('/') + "/" + fileData.Name, FileMode.Open);

        if (textStream == null)
            return Task.FromResult("");

        var streamReader = new StreamReader(textStream);

        var text = streamReader.ReadToEnd();

        streamReader.Close();
        textStream.Close();

        return Task.FromResult(text);
    }

    public override Task Write(FileData fileData, string content)
    {
        EnsureConnect();

        var textStream = Client.Open(InternalPath.TrimEnd('/') + "/" + fileData.Name, FileMode.Create);

        var streamWriter = new StreamWriter(textStream);
        streamWriter.Write(content);

        streamWriter.Flush();
        textStream.Flush();

        streamWriter.Close();
        textStream.Close();

        return Task.CompletedTask;
    }

    public override async Task Upload(string name, Stream stream, Action<int>? progressUpdated = null)
    {
        var dataStream = new SyncStreamAdapter(stream);
        
        await Task.Factory.FromAsync((x, _) => Client.BeginUploadFile(dataStream, InternalPath + name, x, null, u =>
            {
                progressUpdated?.Invoke((int)((long)u / stream.Length));
            }),
            Client.EndUploadFile, null);
    }

    public override Task MkDir(string name)
    {
        Client.CreateDirectory(InternalPath + name);

        return Task.CompletedTask;
    }

    public override Task<string> Pwd()
    {
        return Task.FromResult(CurrentPath);
    }

    public override Task<string> DownloadUrl(FileData fileData)
    {
        throw new NotImplementedException();
    }

    public override Task<Stream> DownloadStream(FileData fileData)
    {
        var stream = new MemoryStream(100 * 1024 * 1024);
        Client.DownloadFile(InternalPath + fileData.Name, stream);

        return Task.FromResult<Stream>(stream);
    }

    public override Task Delete(FileData fileData)
    {
        Client.Delete(InternalPath + fileData.Name);

        return Task.CompletedTask;
    }

    public override Task Move(FileData fileData, string newPath)
    {
        Client.RenameFile(InternalPath + fileData.Name, InternalPath + newPath);

        return Task.CompletedTask;
    }

    public override Task Compress(params FileData[] files)
    {
        throw new NotImplementedException();
    }

    public override Task Decompress(FileData fileData)
    {
        throw new NotImplementedException();
    }

    public override Task<string> GetLaunchUrl()
    {
        return Task.FromResult($"sftp://{SftpUser}@{SftpHost}:{SftpPort}");
    }

    public override object Clone()
    {
        return new SftpFileAccess(SftpHost, SftpUser, SftpPassword, SftpPort, ForceUserDir);
    }
}