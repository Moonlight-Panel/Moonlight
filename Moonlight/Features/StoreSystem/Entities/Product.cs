using Moonlight.Core.Database.Enums;

namespace Moonlight.Features.StoreSystem.Entities;

public class Product
{
    public int Id { get; set; }
    public Category Category { get; set; }

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Slug { get; set; } = "";
    
    public double Price { get; set; }
    public int Stock { get; set; }
    public int MaxPerUser { get; set; }
    public int Duration { get; set; }
    
    public ServiceType Type { get; set; }
    public string ConfigJson { get; set; } = "{}";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}