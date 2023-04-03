namespace Moonlight.App.Helpers.Files;

public abstract class FileAccess : ICloneable
{
    public string CurrentPath { get; set; } = "/";
    
    public abstract Task<FileData[]> Ls();
    public abstract Task Cd(string dir);
    public abstract Task Up();
    public abstract Task SetDir(string dir);
    public abstract Task<string> Read(FileData fileData);
    public abstract Task Write(FileData fileData, string content);
    public abstract Task Upload(string name, Stream stream, Action<int>? progressUpdated = null);
    public abstract Task MkDir(string name);
    public abstract Task<string> Pwd();
    public abstract Task<string> DownloadUrl(FileData fileData);
    public abstract Task<Stream> DownloadStream(FileData fileData);
    public abstract Task Delete(FileData fileData);
    public abstract Task Move(FileData fileData, string newPath);
    public abstract Task Compress(params FileData[] files);
    public abstract Task Decompress(FileData fileData);
    public abstract Task<string> GetLaunchUrl();
    public abstract object Clone();
}