using System.ComponentModel.DataAnnotations;

namespace Moonlight.Core.Models.Forms;

public class TwoFactorCodeForm
{
    [Required(ErrorMessage = "You need to provide a code in order to continue")]
    public string Code { get; set; }
}