namespace Moonlight.Shared.Http.Responses.Admin.Sys.Files;

public class FileSystemEntryResponse
{
    public string Name { get; set; }
    public bool IsFile { get; set; }
    public long Size { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}