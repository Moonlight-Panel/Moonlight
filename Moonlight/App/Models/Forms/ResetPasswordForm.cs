using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms;

public class ResetPasswordForm
{
    [Required(ErrorMessage = "You need to specify an email address")]
    [EmailAddress]
    public string Email { get; set; } = "";
}