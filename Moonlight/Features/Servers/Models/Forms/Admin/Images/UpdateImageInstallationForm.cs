using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Images;

public class UpdateImageInstallationForm
{
    [Required(ErrorMessage = "You need to specify a install docker image")]
    [Description("This specifies the docker image to use for the script execution")]
    public string InstallDockerImage { get; set; }
    
    [Required(ErrorMessage = "You need to specify a install shell")]
    [Description("This is the shell to pass the install script to")]
    public string InstallShell { get; set; }
    
    [Required(ErrorMessage = "You need to specify a install script")]
    public string InstallScript { get; set; }
}