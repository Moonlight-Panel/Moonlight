using MoonCore.Helpers;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

namespace Moonlight.Core.Helpers;

public class HostFileActions : IFileActions
{
    private readonly string RootDirectory;

    public HostFileActions(string rootDirectory)
    {
        RootDirectory = rootDirectory;
    }

    public Task<FileEntry[]> List(string path)
    {
        var fullPath = GetFullPath(path);
        var entries = new List<FileEntry>();

        if (Directory.Exists(fullPath))
        {
            entries.AddRange(Directory.GetDirectories(fullPath)
                .Select(dir => new FileEntry
                {
                    Name = Path.GetFileName(dir),
                    IsDirectory = true,
                    LastModifiedAt = Directory.GetLastWriteTime(dir)
                }));

            entries.AddRange(Directory.GetFiles(fullPath)
                .Select(file => new FileEntry
                {
                    Name = Path.GetFileName(file),
                    Size = new FileInfo(file).Length,
                    IsFile = true,
                    LastModifiedAt = File.GetLastWriteTime(file)
                }));
        }

        return Task.FromResult(entries.ToArray());
    }

    public Task DeleteFile(string path)
    {
        var fullPath = GetFullPath(path);
        
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task DeleteDirectory(string path)
    {
        var fullPath = GetFullPath(path);
        
        if (Directory.Exists(fullPath))
            Directory.Delete(fullPath, true);
        
        return Task.CompletedTask;
    }

    public Task Move(string from, string to)
    {
        var source = GetFullPath(from);
        var destination = GetFullPath(to);

        if (File.Exists(source))
            File.Move(source, destination);
        else
            Directory.Move(source, destination);

        return Task.CompletedTask;
    }

    public Task CreateDirectory(string path)
    {
        var fullPath = GetFullPath(path);
        Directory.CreateDirectory(fullPath);
        return Task.CompletedTask;
    }

    public Task CreateFile(string path)
    {
        var fullPath = GetFullPath(path);
        File.Create(fullPath).Close();
        return Task.CompletedTask;
    }

    public Task<string> ReadFile(string path)
    {
        var fullPath = GetFullPath(path);
        return File.ReadAllTextAsync(fullPath);
    }

    public Task WriteFile(string path, string content)
    {
        var fullPath = GetFullPath(path);
        
        EnsureDir(fullPath);
        
        File.WriteAllText(fullPath, content);
        return Task.CompletedTask;
    }

    public Task<Stream> ReadFileStream(string path)
    {
        var fullPath = GetFullPath(path);
        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public Task WriteFileStream(string path, Stream dataStream)
    {
        var fullPath = GetFullPath(path);
        
        EnsureDir(fullPath);
        
        using (var fileStream = File.Create(fullPath))
            dataStream.CopyTo(fileStream);

        return Task.CompletedTask;
    }

    private void EnsureDir(string path)
    {
        var pathWithoutFileName = Formatter.ReplaceEnd(path, Path.GetFileName(path), "");
        Directory.CreateDirectory(pathWithoutFileName);
    }

    public IFileActions Clone()
    {
        return new HostFileActions(RootDirectory);
    }

    private string GetFullPath(string path)
    {
        return Path.GetFullPath(Path.Combine(RootDirectory, path.TrimStart('/')));
    }

    public void Dispose()
    {
    }
}