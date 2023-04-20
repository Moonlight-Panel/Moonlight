using Moonlight.App.Models.Misc;

namespace Moonlight.App.Database.Entities;

public class SupportChatMessage
{
    public int Id { get; set; }
    
    public string Content { get; set; } = "";
    public string Attachment { get; set; } = "";

    public User? Sender { get; set; }
    public User Recipient { get; set; }

    public bool IsQuestion { get; set; } = false;
    public QuestionType QuestionType { get; set; }
    public string Answer { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}