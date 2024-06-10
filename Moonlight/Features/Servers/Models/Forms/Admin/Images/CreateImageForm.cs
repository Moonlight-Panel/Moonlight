using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MoonCoreUI.Attributes;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Images;

public class CreateImageForm
{
    // General
    [Required(ErrorMessage = "You need to provide a name")]
    [Page("General")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "You need to provide an author")]
    [Page("General")]
    public string Author { get; set; }
    
    [Description("A http(s) url directly to a json file which will serve as an update for the image. When a update is fetched, it will just get this url and try to load it")]
    [Page("General")]
    public string? UpdateUrl { get; set; }
    
    [Description("Provide a url here in order to give people the ability to donate for your work")]
    [Page("General")]
    public string? DonateUrl { get; set; }
    
    // Start, Stop & Status
    [Required(ErrorMessage = "You need to specify a startup command")]
    [Description("This command will be executed at the start of a server. You can use environment variables in a {} here")]
    [Page("Startup, Stop & Status")]
    public string StartupCommand { get; set; } = "echo Startup command here";
    
    [Required(ErrorMessage = "You need to specify a regex online detection string")]
    [Description("A regex string specifying that a server is online when the daemon finds a match in the console output matching this expression")]
    [Page("Startup, Stop & Status")]
    public string OnlineDetection { get; set; } = "I am online";
    
    [Required(ErrorMessage = "You need to specify a stop command")]
    [Description("A command which will be sent to the servers stdin when it should get stopped. Power signals can be achived by using ^. E.g. ^C")]
    [Page("Startup, Stop & Status")]
    public string StopCommand { get; set; } = "^C";
    
    // Installation
    [Required(ErrorMessage = "You need to specify a install shell")]
    [Description("The path to the shell which should execute the install script")]
    [Page("Installation")]
    public string InstallShell { get; set; } = "/bin/bash";
    
    [Required(ErrorMessage = "You need to specify the install docker image")]
    [Description("This specifies the image where the install script will be started in")]
    [Page("Installation")]
    public string InstallDockerImage { get; set; } = "debian:latest";
    
    [Required(ErrorMessage = "You need to provide a install script")]
    [Page("Installation")]
    [CustomFormComponent("InstallScriptEditor")]
    public string InstallScript { get; set; } = "#! /bin/bash\necho Done";

    [Range(1, 100, ErrorMessage = "You need to specify a valid amount of allocations")]
    [Description("This specifies the amount of allocations needed for this image in order to create a server")]
    [Page("Installation")]
    public int AllocationsNeeded { get; set; } = 1;
}