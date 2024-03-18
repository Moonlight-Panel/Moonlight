using Moonlight.Features.Servers.Api.Packets;
using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Api.Resources;

public class ServerListItem
{
    public int Id { get; set; }
    public ServerState State { get; set; }
    public ServerStats Stats { get; set; }
}