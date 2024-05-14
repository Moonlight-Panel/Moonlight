namespace Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

public interface IArchiveFileActions
{
    public Task Archive(string path, string[] files);
    public Task Extract(string path, string destination);
}