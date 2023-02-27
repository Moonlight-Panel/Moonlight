using CloudFlare.Client.Enumerators;

namespace Moonlight.App.Models.Misc;

public class DnsRecord
{
    public string Name { get; set; }
    public string Content { get; set; }
    public DnsRecordType Type { get; set; }
    public string Id { get; set; }
    public bool Proxied { get; set; }
    public int Priority { get; set; }
    public int Ttl { get; set; }
}