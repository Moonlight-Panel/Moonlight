using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Event.Args;

namespace Moonlight.App.Event;

public class Events
{
    public static EventHandler<User> OnUserRegistered;
    public static EventHandler<User> OnUserPasswordChanged;
    public static EventHandler<User> OnUserTotpSet;
    public static EventHandler<MailVerificationEventArgs> OnUserMailVerify;
    public static EventHandler<Service> OnServiceOrdered;
}