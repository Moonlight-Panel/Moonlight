using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Moonlight.Features.Servers.Entities.Enums;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Images.Variables;

public class UpdateImageVariable
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
    public bool AllowEdit { get; set; } = false;

    [Description("Allow the user to view the variable but not edit it unless specified otherwise")]
    public bool AllowView { get; set; } = false;
    
    [Description(
        "Specifies the type of the variable. This specifies what ui the user will see for the variable. You can also specify the options which are available using the filter field")]
    public ServerImageVariableType Type { get; set; } = ServerImageVariableType.Text;
    
    [Description("(Optional)\nText: A regex filter which will check if the user input mathes a correct variable value\nSelect: Specify the available values seperated by a semicolon")]
    public string Filter { get; set; }
}