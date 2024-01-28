using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Images;

public class ParseConfigOptionForm
{
    [Required(ErrorMessage = "You need to specify the key of an option")]
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}