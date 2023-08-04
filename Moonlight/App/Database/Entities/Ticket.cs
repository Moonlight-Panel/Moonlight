using Moonlight.App.Models.Misc;

namespace Moonlight.App.Database.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string IssueTopic { get; set; } = "";
    public string IssueDescription { get; set; } = "";
    public string IssueTries { get; set; } = "";
    public User CreatedBy { get; set; }
    public User? AssignedTo { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public TicketSubject Subject { get; set; }
    public int SubjectId { get; set; }
    public List<TicketMessage> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}