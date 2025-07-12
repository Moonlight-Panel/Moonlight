namespace Moonlight.Shared.Http.Responses.Admin.Sys;

public class FileSystemEntryResponse
{
    public string Name { get; set; }
    public bool IsFolder { get; set; }
    public long Size { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}