namespace Moonlight.App.Helpers.Files;

public interface IFileAccess
{
    public Task<FileData[]> Ls();
    public Task Cd(string dir);
    public Task Up();
    public Task SetDir(string dir);
    public Task<string> Read(FileData fileData);
    public Task Write(FileData fileData, string content);
    public Task Upload(string name, Stream stream, Action<int>? progressUpdated = null);
    public Task MkDir(string name);
    public Task<string> Pwd();
    public Task<string> DownloadUrl(FileData fileData);
    public Task<Stream> DownloadStream(FileData fileData);
    public Task Delete(FileData fileData);
    public Task Move(FileData fileData, string newPath);
    public Task<string> GetLaunchUrl();
}