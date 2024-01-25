using Moonlight.Core.Database.Entities;

namespace Moonlight.Core.Event.Args;

public class MailVerificationEventArgs
{
    public User User { get; set; }
    public string Jwt { get; set; }
}