namespace Moonlight.Shared.Http.Requests.Admin.Sys.Files;

public class CompressRequest
{
    public string Type { get; set; }
    public string Path { get; set; }
    public string[] ItemsToCompress { get; set; }
}