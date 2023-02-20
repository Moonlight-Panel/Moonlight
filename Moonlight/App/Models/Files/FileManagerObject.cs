namespace Moonlight.App.Models.Files;

public class FileManagerObject
{
    public string Name { get; set; }
    public bool IsFile { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}