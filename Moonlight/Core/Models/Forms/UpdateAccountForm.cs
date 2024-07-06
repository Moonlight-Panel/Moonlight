using System.ComponentModel.DataAnnotations;

namespace Moonlight.Core.Models.Forms;

public class UpdateAccountForm
{
    public string Username { get; set; }
    public string Email { get; set; } = "";
}