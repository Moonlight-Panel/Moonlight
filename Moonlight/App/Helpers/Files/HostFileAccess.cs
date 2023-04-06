using System.IO.Compression;

namespace Moonlight.App.Helpers.Files;

public class HostFileAccess : FileAccess
{
    private string BasePath;

    public HostFileAccess(string basePath)
    {
        BasePath = basePath;
    }
    private string currhp => BasePath.TrimEnd('/') + "/" + CurrentPath.TrimStart('/').TrimEnd('/') + "/";
    public override async Task<FileData[]> Ls()
    {
        var x = new List<FileData>();
        
        foreach (var dir in Directory.GetDirectories(currhp))
        {
            x.Add(new()
            {
                Name = dir.Remove(0, currhp.Length),
                Size = 0,
                IsFile = false,
            });
        }
        
        foreach (var fn in Directory.GetFiles(currhp))
        {
            x.Add(new()
            {
                Name = fn.Remove(0, currhp.Length),
                Size = new FileInfo(fn).Length,
                IsFile = true,
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
        return await File.ReadAllTextAsync(currhp + fileData.Name);
    }

    public override async Task Write(FileData fileData, string content)
    {
        await File.WriteAllTextAsync(currhp + fileData.Name, content);
    }

    public override async Task Upload(string name, Stream dataStream, Action<int>? progressUpdated = null)
    {
        var ms = new MemoryStream();
        await dataStream.CopyToAsync(ms);
        var data = ms.ToArray();
        ms.Dispose();
        dataStream.Dispose();

        await File.WriteAllBytesAsync(currhp + name, data);
    }

    public override async Task MkDir(string name)
    {
        Directory.CreateDirectory(currhp + name + "/");
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
        var s = new MemoryStream(8 * 1024 * 1204); //TODO: Add config option
        
        s.Write(File.ReadAllBytes(currhp + fileData.Name));
        s.Position = 0;

        return s;
    }

    public override async Task Delete(FileData fileData)
    {
        if (fileData.IsFile)
            File.Delete(currhp + fileData.Name);
        else
            Directory.Delete(currhp + fileData.Name);
    }

    public override async Task Move(FileData fileData, string newPath)
    {
        if (fileData.IsFile)
            File.Move(currhp + fileData.Name, newPath);
        else
            Directory.Move(currhp + fileData.Name, newPath);
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
        throw new NotImplementedException();
    }

    public override object Clone()
    {
        return new HostFileAccess(BasePath);
    }
}