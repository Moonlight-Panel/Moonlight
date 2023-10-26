using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms.Admin.Store;

public class AddGiftCodeForm
{
    [MinLength(5, ErrorMessage = "The code needs to be longer than 4")]
    [MaxLength(15, ErrorMessage = "The code should not be longer than 15 characters")]
    public string Code { get; set; } = "";
    
    [Range(0, int.MaxValue, ErrorMessage = "The value needs to be equals or greater than 0")]
    public double Value { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "The amount needs to be equals or greater than 0")]
    public int Amount { get; set; }
}