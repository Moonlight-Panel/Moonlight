using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms.Store;

public class EditCategoryForm
{
    [Required(ErrorMessage = "You need to specify a name")]
    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    [Required(ErrorMessage = "You need to specify a slug")]
    [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "You need to enter a valid slug only containing lowercase characters and numbers")]
    public string Slug { get; set; } = "";
}