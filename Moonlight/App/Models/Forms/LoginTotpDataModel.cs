using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms;

public class LoginTotpDataModel
{
    [Required(ErrorMessage = "You need to enter a 2fa code")]
    public string Code { get; set; } = "";
}