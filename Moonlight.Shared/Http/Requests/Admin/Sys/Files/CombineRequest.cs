using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Admin.Sys.Files;

public class CombineRequest
{
    [Required(ErrorMessage = "Destination is required")]
    public string Destination { get; set; }
    
    [Required(ErrorMessage = "Files are required")]
    public string[] Files { get; set; }
}