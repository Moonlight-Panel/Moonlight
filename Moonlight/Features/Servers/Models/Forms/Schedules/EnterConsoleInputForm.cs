using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Schedules;

public class EnterConsoleInputForm
{
    [Required(ErrorMessage = "You need to specify a input")]
    [Description("This input specifies what will be sent to the servers console")]
    public string Input { get; set; }
}