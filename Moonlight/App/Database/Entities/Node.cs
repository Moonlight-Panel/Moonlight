namespace Moonlight.App.Database.Entities;

public class Node
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Fqdn { get; set; } = "";
    public string TokenId { get; set; } = "";
    public string Token { get; set; } = "";
    public int SftpPort { get; set; }
    public int HttpPort { get; set; }
    public int MoonlightDaemonPort { get; set; }
    public List<NodeAllocation> Allocations { get; set; } = new();
    public bool Ssl { get; set; } = false;
}