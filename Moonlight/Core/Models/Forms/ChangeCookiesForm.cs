using System.ComponentModel;

namespace Moonlight.Core.Models.Forms;

public class ChangeCookiesForm
{
    [Description("This specifies if you would like to personalize your experience with optional cookies.")]
    public bool UseOptionalCookies { get; set; } = false;
}