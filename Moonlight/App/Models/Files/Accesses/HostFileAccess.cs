using Logging.Net;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;

namespace Moonlight.App.Models.Files.Accesses;

public class HostFileAccess : IFileAccess
{
    private readonly string BasePath;
    private string RealPath => BasePath + Path;
    private string Path = "/";

    public HostFileAccess(string bp)
    {
        BasePath = bp;
    }
    
    public Task<FileManagerObject[]> GetDirectoryContent()
    {
        var x = new List<FileManagerObject>();

        foreach (var directory in Directory.EnumerateDirectories(RealPath))
        {
            x.Add(new ()
            {
                Name = System.IO.Path.GetFileName(directory)!,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Size = 0,
                IsFile = false
            });
        }
        
        foreach (var file in Directory.GetFiles(RealPath))
        {
            x.Add(new ()
            {
                Name = System.IO.Path.GetFileName(file)!,
                CreatedAt = File.GetCreationTimeUtc(file),
                UpdatedAt = File.GetLastWriteTimeUtc(file),
                Size = new System.IO.FileInfo(file).Length,
                IsFile = File.Exists(file)
            });
        }
        
        return Task.FromResult(x.ToArray());
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

    public Task<string> ReadFile(FileManagerObject fileManagerObject)
    {
        return Task.FromResult(File.ReadAllText(RealPath + fileManagerObject.Name));
    }

    public Task WriteFile(FileManagerObject fileManagerObject, string content)
    {
        File.WriteAllText(RealPath + fileManagerObject.Name, content);
        return Task.CompletedTask;
    }

    public async Task UploadFile(string name, Stream stream, Action<int>? progressUpdated = null)
    {
        var fs = File.OpenWrite(RealPath + name);
        
        var dataStream = new StreamProgressHelper(stream)
        {
            Progress = i =>
            {
                if (progressUpdated != null)
                    progressUpdated.Invoke(i);
            }
        };

        await dataStream.CopyToAsync(fs);
        await fs.FlushAsync();
        fs.Close();
    }

    public Task CreateDirectory(string name)
    {
        Directory.CreateDirectory(RealPath + name);
        return Task.CompletedTask;
    }

    public Task<string> GetCurrentPath()
    {
        return Task.FromResult(Path);
    }

    public Task<Stream> GetDownloadStream(FileManagerObject managerObject)
    {
        var stream = new FileStream(RealPath + managerObject.Name, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream>(stream);
    }

    public Task<string> GetDownloadUrl(FileManagerObject managerObject)
    {
        throw new NotImplementedException();
    }

    public Task Delete(FileManagerObject managerObject)
    {
        if(managerObject.IsFile)
            File.Delete(RealPath + managerObject.Name);
        else
            Directory.Delete(RealPath + managerObject.Name, true);
        
        return Task.CompletedTask;
    }

    public Task Move(FileManagerObject managerObject, string newPath)
    {
        if(managerObject.IsFile)
            File.Move(RealPath + managerObject.Name, BasePath + newPath);
        else
            Directory.Move(RealPath + managerObject.Name, BasePath + newPath);
        
        return Task.CompletedTask;
    }

    public Task<string> GetLaunchUrl()
    {
        throw new DisplayException("WinSCP cannot be launched here");
    }
}