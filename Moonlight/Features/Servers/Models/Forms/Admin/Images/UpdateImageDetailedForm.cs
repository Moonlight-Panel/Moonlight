using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Images;

public class UpdateImageDetailedForm
{
    [Required(ErrorMessage = "You need to provide a name")]
    public string Name { get; set; } = "";
    [Required(ErrorMessage = "You need to provide a author name")]
    public string Author { get; set; } = "";
    
    [Description("A http(s) url directly to a json file which will serve as an update for the image. When a update is fetched, it will just get this url and try to load it")]
    public string? UpdateUrl { get; set; }
    [Description("Provide a url here in order to give people the ability to donate for your work")]
    public string? DonateUrl { get; set; }

    [Required(ErrorMessage = "You need to specify a startup command")]
    [Description("This command will be executed at the start of a server. You can use environment variables in a {} here")]
    public string StartupCommand { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify a regex online detection string")]
    [Description("A regex string specifying that a server is online when the daemon finds a match in the console output matching this expression")]
    public string OnlineDetection { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify a stop command")]
    [Description("A command which will be sent to the servers stdin when it should get stopped. Power signals can be achived by using ^. E.g. ^C")]
    public string StopCommand { get; set; } = "";

    [Required(ErrorMessage = "You need to specify a install shell")]
    [Description("The path to the shell which should execute the install script")]
    public string InstallShell { get; set; } = "";
    [Required(ErrorMessage = "You need to specify the install docker image")]
    [Description("This specifies the image where the install script will be started in")]
    public string InstallDockerImage { get; set; } = "";
    [Required(ErrorMessage = "You need to provide a install script")]
    public string InstallScript { get; set; } = "";

    [Range(1, 100, ErrorMessage = "You need to specify a valid amount of allocations")]
    [Description("This specifies the amount of allocations needed for this image in order to create a server")]
    public int AllocationsNeeded { get; set; } = 1;
    
    public int DefaultDockerImage { get; set; } = 0;
    
    [Description("This toggle specifies if a user is allowed to change the docker image from the list of docker images associated to the image")]
    public bool AllowDockerImageChange { get; set; } = false;
}