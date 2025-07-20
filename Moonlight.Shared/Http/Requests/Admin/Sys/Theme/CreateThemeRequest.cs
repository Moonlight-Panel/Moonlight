using System.ComponentModel.DataAnnotations;
using Moonlight.Shared.Misc;

namespace Moonlight.Shared.Http.Requests.Admin.Sys.Theme;

public class CreateThemeRequest
{
    [Required(ErrorMessage = "You need to provide a name")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "You need to provide an author")]
    public string Author { get; set; }
    
    [Required(ErrorMessage = "You need to provide a version")]
    public string Version { get; set; }
    
    public string? UpdateUrl { get; set; }
    public string? DonateUrl { get; set; }
    
    public ApplicationTheme Content { get; set; }
}