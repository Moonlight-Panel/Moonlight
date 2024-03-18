using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Users.Schedules;

public class CreateScheduleForm
{
    [Required(ErrorMessage = "You need to provide a name for this schedule")]
    public string Name { get; set; }
}