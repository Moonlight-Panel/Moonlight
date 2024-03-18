using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Images;

public class CreateImageForm
{
    [Required(ErrorMessage = "You need to provide a name")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "You need to provide an author")]
    public string Author { get; set; }
}