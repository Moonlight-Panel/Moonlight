using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Images;

public class UpdateImageForm
{
    [Required(ErrorMessage = "You need to specify a name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "You need to specify a author")]
    public string Author { get; set; }
    
    [Description("The donate button on your image will lead to the page you specify here")]
    public string? DonateUrl { get; set; }
    
    [Description("This field allows you to specify an direct http(s) url to fetch image updates from")]
    public string? UpdateUrl { get; set; }
}