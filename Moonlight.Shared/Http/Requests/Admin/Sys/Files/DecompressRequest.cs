namespace Moonlight.Shared.Http.Requests.Admin.Sys.Files;

public class DecompressRequest
{
    public string Format { get; set; }
    public string Path { get; set; }
    public string Destination { get; set; }
}