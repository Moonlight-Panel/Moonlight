using System.ComponentModel.DataAnnotations;
using Moonlight.App.Models.Misc;

namespace Moonlight.App.Models.Forms;

public class CreateTicketDataModel
{
    [Required(ErrorMessage = "You need to specify a issue topic")]
    [MinLength(5, ErrorMessage = "The issue topic needs to be longer than 5 characters")]
    public string IssueTopic { get; set; }
    
    [Required(ErrorMessage = "You need to specify a issue description")]
    [MinLength(10, ErrorMessage = "The issue description needs to be longer than 10 characters")]
    public string IssueDescription { get; set; }
    
    [Required(ErrorMessage = "You need to specify your tries to solve this issue")]
    public string IssueTries { get; set; }
    
    public TicketSubject Subject { get; set; }
    public int SubjectId { get; set; }
}