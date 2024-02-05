namespace Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

public interface IFileCompressAccess
{
    public Task Compress(string[] names);
    public Task Decompress(string name);
}