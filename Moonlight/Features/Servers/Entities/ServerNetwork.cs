using Moonlight.Core.Database.Entities;

namespace Moonlight.Features.Servers.Entities;

public class ServerNetwork
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public User User { get; set; }
    public ServerNode Node { get; set; }
}