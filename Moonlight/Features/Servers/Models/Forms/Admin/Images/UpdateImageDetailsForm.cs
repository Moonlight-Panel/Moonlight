using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Images;

public class UpdateImageDetailsForm
{
    [Description("The allocations (aka. ports) a image needs in order to be created")]
    [Range(1, int.MaxValue)]
    public int AllocationsNeeded { get; set; }
    
    [Required(ErrorMessage = "You need to specify a startup command")]
    [Description("This command gets passed to the container of the image to execute")]
    public string StartupCommand { get; set; }
    
    [Required(ErrorMessage = "You need to specify a stop command")]
    [Description("This command will get written into the input stream of the server process when the server should get stopped")]
    public string StopCommand { get; set; }
    
    [Required(ErrorMessage = "You need to specify a online detection")]
    [Description("The regex string you specify here will be used in order to detect if a server is up and running")]
    public string OnlineDetection { get; set; }
}