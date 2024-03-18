using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Images.Parsing;

public class ParseConfigForm
{
    [Required(ErrorMessage = "You need to specify a type in a parse configuration")]
    public string Type { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify a file in a parse configuration")]
    public string File { get; set; } = "";
}