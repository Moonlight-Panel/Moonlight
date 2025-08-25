using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Admin.Sys.Files;

public class DecompressRequest
{
    [Required(ErrorMessage = "You need to provide a format")]
    public string Format { get; set; }
    
    [Required(ErrorMessage = "You need to provide a path")]
    public string Path { get; set; }
    
    [Required(ErrorMessage = "You need to provide a destination")]
    public string Destination { get; set; }
}