namespace Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

public interface IFileActions : IDisposable
{
    public Task<FileEntry[]> List(string path);
    public Task Delete(string path);
    public Task Move(string from, string to);
    public Task CreateDirectory(string name);
    public Task CreateFile(string name);
    public Task<string> ReadFile(string name);
    public Task WriteFile(string name, string content);
    public Task<Stream> ReadFileStream(string name);
    public Task WriteFileStream(string name, Stream dataStream);
    public IFileActions Clone();
}