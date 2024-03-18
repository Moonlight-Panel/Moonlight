using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Images.DockerImages;

public class CreateDockerImage
{
    [Required(ErrorMessage = "You need to specify a docker image name")]
    [Description("This is the name of the docker image. E.g. moonlightpanel/moonlight:canary")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "You need to specify a display name")]
    [Description("This will be shown if the user is able to change the docker image as the image name")]
    public string DisplayName { get; set; }
    
    [Description("Specifies if the docker image should be pulled/updated when creating a server instance. Disable this for only local existing docker images")]
    public bool AutoPull { get; set; } = true;
}