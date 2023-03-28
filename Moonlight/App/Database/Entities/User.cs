using System.ComponentModel.DataAnnotations;
using Moonlight.App.Models.Misc;

namespace Moonlight.App.Database.Entities;

public class User
{
    public int Id { get; set; }
    
    // Personal data
    
    [Required]
    [MinLength(3, ErrorMessage = "Invalid first name")]
    [MaxLength(64, ErrorMessage = "Max lenght reached")]
    public string FirstName { get; set; } = "";
    
    [Required]
    [MinLength(3, ErrorMessage = "Invalid last name")]
    [MaxLength(64, ErrorMessage = "Max lenght reached")]
    public string LastName { get; set; } = "";
    
    [Required(ErrorMessage = "You need to enter an email address")]
    [EmailAddress(ErrorMessage = "You need to enter a valid email address")]
    public string Email { get; set; } = "";
    
    [Required(ErrorMessage = "You need to enter a password")]
    [MinLength(8, ErrorMessage = "You need to enter a password with minimum 8 characters in lenght")]
    public string Password { get; set; } = "";
    
    [Required]
    [RegularExpression(@"^(?:[A-Z] \d|[^\W\d_]{2,}\.?)(?:[- &#39;’][^\W\d_]+\.?)*\s+[1-9]\d{0,3} ?[a-zA-Z]?(?: ?[/-] ?[1-9]\d{0,3} ?[a-zA-Z]?)?$", 
        ErrorMessage = "Street and house number required")]
    [MaxLength(128, ErrorMessage = "Max lenght reached")]
    public string Address { get; set; } = "";
    
    [Required]
    [MinLength(3, ErrorMessage = "Invalid city")]
    [MaxLength(128, ErrorMessage = "Max lenght reached")]
    public string City { get; set; } = "";
    
    [Required]
    [MinLength(3, ErrorMessage = "Invalid state")]
    [MaxLength(64, ErrorMessage = "Max lenght reached")]
    public string State { get; set; } = "";
    
    [Required]
    [MinLength(3, ErrorMessage = "Invalid country")]
    [MaxLength(64, ErrorMessage = "Max lenght reached")]
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