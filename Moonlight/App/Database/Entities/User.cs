using System.ComponentModel.DataAnnotations;
using Moonlight.App.Models.Misc;

namespace Moonlight.App.Database.Entities;

public class User
{
    public int Id { get; set; }
    
    // Personal data
    
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    
    [Required(ErrorMessage = "You need to enter an email address")]
    [EmailAddress(ErrorMessage = "You need to enter a valid email address")]
    public string Email { get; set; } = "";
    
    [Required(ErrorMessage = "You need to enter a password")]
    [MinLength(8, ErrorMessage = "You need to enter a password with minimum 8 characters in lenght")]
    public string Password { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Country { get; set; } = "";
    
    // States
    
    public UserStatus Status { get; set; } = UserStatus.Unverified;
    public bool Admin { get; set; } = false;
    public bool SupportPending { get; set; } = false;
    
    // Security
    public bool TotpEnabled { get; set; } = false;
    public string TotpSecret { get; set; } = "";
    public DateTime TokenValidTime { get; set; } = DateTime.Now;
    
    // Discord
    public long DiscordId { get; set; } = -1;
    
    // Date stuff
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Subscriptions
    public Subscription? Subscription { get; set; } = null;
    public DateTime? SubscriptionSince { get; set; }
    public int SubscriptionDuration { get; set; }
}