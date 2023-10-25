using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms.Admin.Store;

public class EditCouponForm
{
    [MinLength(5, ErrorMessage = "The code needs to be longer than 4")]
    [MaxLength(15, ErrorMessage = "The code should not be longer than 15 characters")]
    public string Code { get; set; } = "";
    
    [Range(1, 99, ErrorMessage = "The percent needs to be between 1 and 99")]
    public int Percent { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "The amount needs to be equals or greater than 0")]
    public int Amount { get; set; }
}