using MoonCore.Helpers;

namespace Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

public class BaseFileAccess : IDisposable
{
    public readonly IFileActions Actions;

    public string CurrentDirectory { get; private set; } = "/";

    public BaseFileAccess(IFileActions actions)
    {
        Actions = actions;
    }

    public async Task<FileEntry[]> List() => await Actions.List(CurrentDirectory);

    public Task ChangeDirectory(string relativePath)
    {
        if (relativePath == "..")
        {
            if (CurrentDirectory == "/")
                return Task.CompletedTask;
            
            CurrentDirectory = "/" 
                               + string.Join('/', CurrentDirectory.Split("/").Where(x => !string.IsNullOrEmpty(x)).SkipLast(1))
                               + "/";

            CurrentDirectory = CurrentDirectory.Replace("//", "/");
        }
        else
            CurrentDirectory = CurrentDirectory + relativePath + "/";
        
        return Task.CompletedTask;
    }

    public Task SetDirectory(string path)
    {
        CurrentDirectory = path;
        
        return Task.CompletedTask;
    }

    public Task<string> GetCurrentDirectory()
    {
        return Task.FromResult(CurrentDirectory);
    }

    public async Task Delete(FileEntry entry)
    {
        var finalPath = CurrentDirectory + entry.Name;

        if(entry.IsFile)
            await Actions.DeleteFile(finalPath);
        else
            await Actions.DeleteDirectory(finalPath);
    }

    public async Task Move(FileEntry entry, string to)
    {
        var finalPathFrom = CurrentDirectory + entry.Name;

        await Actions.Move(finalPathFrom, to);
    }

    public async Task Rename(string from, string to)
    {
        var finalPathFrom = CurrentDirectory + from;
        var finalPathTo = CurrentDirectory + to;

        await Actions.Move(finalPathFrom, finalPathTo);
    }

    public async Task CreateDirectory(string name)
    {
        var finalPath = CurrentDirectory + name;

        await Actions.CreateDirectory(finalPath);
    }

    public async Task CreateFile(string name)
    {
        var finalPath = CurrentDirectory + name;

        await Actions.CreateFile(finalPath);
    }

    public async Task<string> ReadFile(string name)
    {
        var finalPath = CurrentDirectory + name;

        return await Actions.ReadFile(finalPath);
    }

    public async Task WriteFile(string name, string content)
    {
        var finalPath = CurrentDirectory + name;

        await Actions.WriteFile(finalPath, content);
    }

    public async Task<Stream> ReadFileStream(string name)
    {
        var finalPath = CurrentDirectory + name;

        return await Actions.ReadFileStream(finalPath);
    }

    public async Task WriteFileStream(string name, Stream dataStream)
    {
        var finalPath = CurrentDirectory + name;

        await Actions.WriteFileStream(finalPath, dataStream);
    }

    public BaseFileAccess Clone()
    {
        return new BaseFileAccess(Actions.Clone())
        {
            CurrentDirectory = CurrentDirectory
        };
    }

    public void Dispose() => Actions.Dispose();
}
