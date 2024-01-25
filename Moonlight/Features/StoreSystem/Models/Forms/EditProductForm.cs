using System.ComponentModel.DataAnnotations;
using Moonlight.Core.Database.Entities.Store;
using Moonlight.Core.Database.Enums;
using Moonlight.Features.StoreSystem.Entities;

namespace Moonlight.Features.StoreSystem.Models.Forms;

public class EditProductForm
{
    [Required(ErrorMessage = "You need to specify a category")]
    public Category Category { get; set; }

    [Required(ErrorMessage = "You need to specify a name")]
    public string Name { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify a description")]
    public string Description { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify a slug")]
    [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "You need to enter a valid slug only containing lowercase characters and numbers")]
    public string Slug { get; set; } = "";
    
    [Range(0, double.MaxValue, ErrorMessage = "The price needs to be equals or above 0")]
    public double Price { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "The stock needs to be equals or above 0")]
    public int Stock { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "The max per user amount needs to be equals or above 0")]
    public int MaxPerUser { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "The duration needs to be equals or above 0")]
    public int Duration { get; set; }
    
    public ServiceType Type { get; set; }
    public string ConfigJson { get; set; } = "{}";
}