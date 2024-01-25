using Moonlight.Features.StoreSystem.Entities;

namespace Moonlight.Core.Database.Entities.Store;

public class Service
{
    public int Id { get; set; }
    public string? Nickname { get; set; }

    public bool Suspended { get; set; } = false;
    
    public Product Product { get; set; }
    public string? ConfigJsonOverride { get; set; }
    
    public User Owner { get; set; }
    public List<ServiceShare> Shares { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime RenewAt { get; set; } = DateTime.UtcNow;
}