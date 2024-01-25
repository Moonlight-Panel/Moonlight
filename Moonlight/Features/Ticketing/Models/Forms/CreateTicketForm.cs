using System.ComponentModel.DataAnnotations;
using Moonlight.Core.Database.Entities.Store;

namespace Moonlight.Features.Ticketing.Models.Forms;

public class CreateTicketForm
{
    [Required(ErrorMessage = "You need to enter a ticket name")]
    [MinLength(8, ErrorMessage = "The title needs to be longer then 8 characters")]
    [MaxLength(64, ErrorMessage = "The ticket name should not exceed 64 characters in lenght")]
    public string Name { get; set; } = "";
    
    [Required(ErrorMessage = "You need to enter a description")]
    [MinLength(8, ErrorMessage = "The description needs to be longer then 8 characters")]
    [MaxLength(256, ErrorMessage = "The description should not exceed 256 characters in lenght")]
    public string Description { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify what you have tried already")]
    [MinLength(8, ErrorMessage = "The tries description needs to be longer then 8 characters")]
    [MaxLength(256, ErrorMessage = "The tries description should not exceed 256 characters in lenght")]
    public string Tries { get; set; } = "";
    
    public Service? Service { get; set; }
}