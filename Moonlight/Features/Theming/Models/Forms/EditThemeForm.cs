using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Theming.Models.Forms;

public class EditThemeForm
{
    [Required(ErrorMessage = "You need to specify a name for your theme")]
    public string Name { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify an author for your theme")]
    public string Author { get; set; } = "";
    
    [Description("Enter a url to date for your theme here in order to show up when other people use this theme")]
    public string? DonateUrl { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify a style sheet url")]
    [Description("A url to your stylesheet")]
    public string CssUrl { get; set; } = "";
    
    [Description("(Optional) A url to your javascript file")]
    public string? JsUrl { get; set; } = null;

    [Description("Enable the theme for this instance")]
    public bool Enabled { get; set; } = false;
}