namespace Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

public class FileEntry
{
    public string Name { get; set; }
    public long Size { get; set; }
    public bool IsFile { get; set; }
    public bool IsDirectory { get; set; }
    public DateTime LastModifiedAt { get; set; }
}