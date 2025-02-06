namespace Moonlight.Shared.Http.Requests.Admin.Sys.Files;

public class DecompressRequest
{
    public string Type { get; set; }
    public string Path { get; set; }
    public string Destination { get; set; }
}