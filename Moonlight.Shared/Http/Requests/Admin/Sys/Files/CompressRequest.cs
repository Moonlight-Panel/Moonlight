using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Admin.Sys.Files;

public class CompressRequest
{
    [Required(ErrorMessage = "Format is required")]
    public string Format { get; set; }
    
    [Required(ErrorMessage = "Destination is required")]
    public string Destination { get; set; }
    
    [Required(ErrorMessage = "Root is required")]
    public string Root { get; set; }
    
    [Required(ErrorMessage = "Items are required")]
    public string[] Items { get; set; }
}