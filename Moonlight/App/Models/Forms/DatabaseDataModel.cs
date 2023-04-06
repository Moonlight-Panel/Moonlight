using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms;

public class DatabaseDataModel
{
    [Required(ErrorMessage = "You need to enter a name")]
    [MinLength(8, ErrorMessage = "The name should be at least 8 characters long")]
    [MaxLength(32, ErrorMessage = "The database name should be maximal 32 characters")]
    [RegularExpression(@"^[a-z0-9]+$", ErrorMessage = "The name should only contain of lower case characters and numbers")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "You need to enter a password")]
    [MinLength(8, ErrorMessage = "The password should be at least 8 characters long")]
    [MaxLength(32, ErrorMessage = "The password name should be maximal 32 characters")]
    public string Password { get; set; } = "";
}