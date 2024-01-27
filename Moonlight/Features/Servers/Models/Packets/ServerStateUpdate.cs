using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Models.Packets;

public class ServerStateUpdate
{
    public int Id { get; set; }
    public ServerState State { get; set; }
}