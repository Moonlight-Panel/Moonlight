namespace Moonlight.Features.FileManager.Models.Abstractions;

public class FileUpload
{
    public string Name { get; set; }
    public Stream Stream { get; set; }
    public long Size { get; set; }
}