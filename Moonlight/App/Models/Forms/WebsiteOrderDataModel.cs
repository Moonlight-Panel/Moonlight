using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms;

public class WebsiteOrderDataModel
{
    [Required(ErrorMessage = "You need to enter a domain")]
    [RegularExpression(@"([a-z0-9|-]+\.)*[a-z0-9|-]+\.[a-z]+", ErrorMessage = "You need to enter a valid domain")]
    public string BaseDomain { get; set; } = "";
}