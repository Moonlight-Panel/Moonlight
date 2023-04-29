using Moonlight.App.Models.Misc;

namespace Moonlight.App.Database.Entities;

public class SupportMessage
{
    public int Id { get; set; }
    public string Message { get; set; } = "";
    public User? Sender { get; set; } = null;
    public User? Recipient { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsQuestion { get; set; } = false;
    public QuestionType Type { get; set; }
    public string Answer { get; set; } = "";
    public bool IsSystem { get; set; } = false;
    public bool IsSupport { get; set; } = false;
}