using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms;

public class UserDataModel
{
    [Required]
    public string FirstName { get; set; } = "";
    
    [Required]
    public string LastName { get; set; } = "";
    
    [Required]
    public string Email { get; set; } = "";
    
    [Required]
    public string Address { get; set; } = "";
    
    [Required]
    public string City { get; set; } = "";
    
    [Required]
    public string State { get; set; } = "";
    
    [Required]
    public string Country { get; set; } = "";
}