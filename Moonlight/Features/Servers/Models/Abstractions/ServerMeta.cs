using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Models.Abstractions;

public class ServerMeta
{
    public ServerState State { get; set; }
    public DateTime LastChangeTimestamp { get; set; }
}