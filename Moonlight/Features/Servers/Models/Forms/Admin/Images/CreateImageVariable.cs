using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Images;

public class CreateImageVariable
{
    [Required(ErrorMessage = "You need to specify a key")]
    [Description("This is the environment variable name")]
    public string Key { get; set; }

    [Description("This is the default value which will be set when a server is created")]
    public string DefaultValue { get; set; } = "";

    [Required(ErrorMessage = "You need to specify a display name")]
    [Description("This is the display name of the variable which will be shown to the user if enabled to edit/view the variable")]
    public string DisplayName { get; set; }

    [Description("This text should describe what the variable does for the user if allowed to view and/or change")] 
    public string Description { get; set; } = "";

    [Description("Allow the user to edit the variable. Wont work if view is disabled")]
    public bool AllowUserToEdit { get; set; } = false;

    [Description("Allow the user to view the variable but not edit it unless specified otherwise")]
    public bool AllowUserToView { get; set; } = false;
}