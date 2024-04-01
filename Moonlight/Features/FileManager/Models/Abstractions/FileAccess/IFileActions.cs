namespace Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

public interface IFileActions : IDisposable
{
    public Task<FileEntry[]> List(string path);
    public Task DeleteFile(string path);
    public Task DeleteDirectory(string path);
    public Task Move(string from, string to);
    public Task CreateDirectory(string path);
    public Task CreateFile(string path);
    public Task<string> ReadFile(string path);
    public Task WriteFile(string path, string content);
    public Task<Stream> ReadFileStream(string path);
    public Task WriteFileStream(string path, Stream dataStream);
    public IFileActions Clone();
}