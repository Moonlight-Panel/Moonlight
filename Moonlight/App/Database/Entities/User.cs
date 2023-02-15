using Moonlight.App.Models.Misc;

namespace Moonlight.App.Database.Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Country { get; set; } = "";
    public UserStatus Status { get; set; } = UserStatus.Unverified;
    public bool TotpEnabled { get; set; }
    public string TotpSecret { get; set; } = "";
    public DateTime TokenValidTime { get; set; } = DateTime.Now;
    public long DiscordId { get; set; }
    public string DiscordUsername { get; set; } = "";
    public string DiscordDiscriminator { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool Admin { get; set; }
}