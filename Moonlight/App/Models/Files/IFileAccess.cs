namespace Moonlight.App.Models.Files;

public interface IFileAccess
{
    public Task<FileManagerObject[]> GetDirectoryContent();
    public Task ChangeDirectory(string s);
    public Task SetDirectory(string s);
    public Task GoUp();
    public Task<string> ReadFile(FileManagerObject fileManagerObject);
    public Task WriteFile(FileManagerObject fileManagerObject, string content);
    public Task UploadFile(string name, Stream stream, Action<int> progressUpdated);
    public Task CreateDirectory(string name);
    public Task<string> GetCurrentPath();
    public Task<Stream> GetDownloadStream(FileManagerObject managerObject);
    public Task<string> GetDownloadUrl(FileManagerObject managerObject);
    public Task Delete(FileManagerObject managerObject);
    public Task Move(FileManagerObject managerObject, string newPath);
    public Task<string> GetLaunchUrl();
}