using System.ComponentModel.DataAnnotations;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Models.Forms;

public class DomainOrderDataModel
{
    [Required(ErrorMessage = "You need to specify a name")]
    [MaxLength(32, ErrorMessage = "The max lenght for the name is 32 characters")]
    [RegularExpression(@"^[a-z]+$", ErrorMessage = "The name should only consist of lower case characters")]
    public string Name { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify a shared domain")]
    public SharedDomain SharedDomain { get; set; }
}