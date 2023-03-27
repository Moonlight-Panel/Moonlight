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
    
    [Required]
    [RegularExpression(@"^((((([a-zA-Z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-zA-Z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)[;]?)+$",
        ErrorMessage = "Must be a valid email")]
    [MaxLength(128, ErrorMessage = "Max lenght reached")]
    public string Email { get; set; } = "";
    
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