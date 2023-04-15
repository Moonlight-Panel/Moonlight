using System.Net;
using System.Text;
using FluentFTP;
using Moonlight.App.Exceptions;

namespace Moonlight.App.Helpers.Files;

public class FtpFileAccess : FileAccess
{
    private string FtpHost, FtpUser, FtpPassword;
    private int FtpPort;

    private AsyncFtpClient Client;
    
    public FtpFileAccess(string ftpHost, int ftpPort, string ftpUser, string ftpPassword)
    {
        FtpHost = ftpHost;
        FtpPort = ftpPort;
        FtpUser = ftpUser;
        FtpPassword = ftpPassword;

        Client = new AsyncFtpClient(FtpHost, FtpUser, FtpPassword, FtpPort);
    }

    private async Task EnsureConnect()
    {
        if (!Client.IsConnected)
            await Client.AutoConnect();
    }
    
    public override async Task<FileData[]> Ls()
    {
        await EnsureConnect();
        
        var x = new List<FileData>();
        
        foreach (FtpListItem item in (await Client.GetListing(CurrentPath)).OrderBy(x => x.Type + " " + x.Name))
        {
            long size = 0;
            
            if (item.Type == FtpObjectType.File)
            {
                size = await Client.GetFileSize(item.FullName);
            }

            x.Add(new()
            {
                Name = item.Name,
                Size = size,
                IsFile = item.Type == FtpObjectType.File,
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
        await EnsureConnect();

        var s = new MemoryStream(8 * 1024 * 1204); //TODO: Add config option
        await Client.DownloadStream(s, CurrentPath.TrimEnd('/') + "/" + fileData.Name);
        var data = s.ToArray();
        await s.DisposeAsync();
        var str = Encoding.UTF8.GetString(data);
        return str;
    }

    public override async Task Write(FileData fileData, string content)
    {
        await EnsureConnect();

        var s = new MemoryStream(8 * 1024 * 1204); //TODO: Add config option
        s.Write(Encoding.UTF8.GetBytes(content));
        s.Position = 0;
        await Client.UploadStream(s, CurrentPath.TrimEnd('/') + "/" + fileData.Name, FtpRemoteExists.Overwrite);
        await s.DisposeAsync();
    }

    public override async Task Upload(string name, Stream dataStream, Action<int>? progressUpdated = null)
    {
        await EnsureConnect();

        IProgress<FtpProgress> progress = new Progress<FtpProgress>(x =>
        {
            progressUpdated?.Invoke((int)x.Progress);
        });
        await Client.UploadStream(dataStream, CurrentPath.TrimEnd('/') + "/" + name, FtpRemoteExists.Overwrite, false, progress);
    }

    public override async Task MkDir(string name)
    {
        await EnsureConnect();

        await Client.CreateDirectory(CurrentPath.TrimEnd('/') + "/" + name + "/");
    }

    public override Task<string> Pwd()
    {
        return Task.FromResult(CurrentPath);
    }

    public override Task<string> DownloadUrl(FileData fileData)
    {
        throw new NotImplementedException();
    }

    public override async Task<Stream> DownloadStream(FileData fileData)
    {
        await EnsureConnect();

        var s = new MemoryStream(8 * 1024 * 1204); //TODO: Add config option
        var downloaded = await Client.DownloadStream(s, CurrentPath.TrimEnd('/') + "/" + fileData.Name);

        if (!downloaded)
            throw new DisplayException("Unable to download file");
        
        return s;
    }

    public override async Task Delete(FileData fileData)
    {
        await EnsureConnect();

        if (fileData.IsFile)
            await Client.DeleteFile(CurrentPath.TrimEnd('/') + "/" + fileData.Name);
        else
            await Client.DeleteDirectory(CurrentPath.TrimEnd('/') + "/" + fileData.Name);
    }

    public override async Task Move(FileData fileData, string newPath)
    {
        await EnsureConnect();

        if (fileData.IsFile)
            await Client.MoveFile(CurrentPath.TrimEnd('/') + "/" + fileData.Name, newPath);
        else
            await Client.MoveDirectory(CurrentPath.TrimEnd('/') + "/" + fileData.Name, newPath);
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
        return Task.FromResult(
                    $"ftp://{FtpUser}:{FtpPassword}@{FtpHost}:{FtpPort}/");
    }

    public override object Clone()
    {
        return new FtpFileAccess(FtpHost, FtpPort, FtpUser, FtpPassword);
    }
}