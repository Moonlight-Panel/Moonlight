using Moonlight.App.Database.Entities;

namespace Moonlight.App.Event.Args;

public class MailVerificationEventArgs
{
    public User User { get; set; }
    public string Jwt { get; set; }
}