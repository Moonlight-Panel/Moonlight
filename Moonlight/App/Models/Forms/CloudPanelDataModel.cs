using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms;

public class CloudPanelDataModel
{
    [Required(ErrorMessage = "You have to enter a name")]
    [MaxLength(32, ErrorMessage = "The name should not be longer than 32 characters")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "You need to specify the host")]
    public string Host { get; set; }
    
    [Required(ErrorMessage = "You need to enter an api url")]
    public string ApiUrl { get; set; }
    
    [Required(ErrorMessage = "You need to enter an api key")]
    public string ApiKey { get; set; }
}