namespace Moonlight.Features.Servers.Models;

public class ServerParseConfig
{
    public string Type { get; set; } = "";
    public string File { get; set; } = "";
    public Dictionary<string, string> Configuration { get; set; } = new();
}