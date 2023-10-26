using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Database.Enums;
using Moonlight.App.Extensions.Attributes;

namespace Moonlight.App.Models.Forms.Store;

public class AddProductForm
{
    [Required(ErrorMessage = "You need to specify a category")]
    [Selector(DisplayProp = "Name", SelectorProp = "Name", UseDropdown = true)]
    public Category Category { get; set; }

    [Required(ErrorMessage = "You need to specify a name")]
    [Description("Teeeeeeeeeeeeeeeeeeeeeeeeeest")]
    public string Name { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify a description")]
    public string Description { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify a slug")]
    [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "You need to enter a valid slug only containing lowercase characters and numbers")]
    public string Slug { get; set; } = "";
    
    [Range(0, double.MaxValue, ErrorMessage = "The price needs to be equals or greater than 0")]
    public double Price { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "The stock needs to be equals or greater than 0")]
    public int Stock { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "The max per user amount needs to be equals or greater than 0")]
    public int MaxPerUser { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "The duration needs to be equals or greater than 0")]
    public int Duration { get; set; }
    
    public ServiceType Type { get; set; }
    public string ConfigJson { get; set; } = "{}";
}