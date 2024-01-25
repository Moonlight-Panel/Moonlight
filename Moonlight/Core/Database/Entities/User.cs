using Moonlight.Core.Database.Entities.Store;
using Moonlight.Features.StoreSystem.Entities;

namespace Moonlight.Core.Database.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Avatar { get; set; } = null;
    public string? TotpKey { get; set; } = null;
    
    // Store
    public double Balance { get; set; }
    public List<Transaction> Transactions { get; set; } = new();

    public List<CouponUse> CouponUses { get; set; } = new();
    public List<GiftCodeUse> GiftCodeUses { get; set; } = new();
    
    // Meta data
    public string Flags { get; set; } = "";
    public int Permissions { get; set; } = 0;
    
    // Timestamps
    public DateTime TokenValidTimestamp { get; set; } = DateTime.UtcNow.AddMinutes(-10);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}