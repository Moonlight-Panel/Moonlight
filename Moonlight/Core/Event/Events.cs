using Moonlight.Core.Database.Entities;
using Moonlight.Core.Event.Args;
using Moonlight.Features.Community.Entities;
using Moonlight.Features.ServiceManagement.Entities;
using Moonlight.Features.Ticketing.Entities;

namespace Moonlight.Core.Event;

public class Events
{
    public static EventHandler<User> OnUserRegistered;
    public static EventHandler<User> OnUserPasswordChanged;
    public static EventHandler<User> OnUserTotpSet;
    public static EventHandler<MailVerificationEventArgs> OnUserMailVerify;
    public static EventHandler<Service> OnServiceOrdered;
    public static EventHandler<TransactionCreatedEventArgs> OnTransactionCreated;
    public static EventHandler<Post> OnPostCreated;
    public static EventHandler<Post> OnPostUpdated;
    public static EventHandler<Post> OnPostDeleted;
    public static EventHandler<Post> OnPostLiked;
    public static EventHandler<PostComment> OnPostCommentCreated;
    public static EventHandler<PostComment> OnPostCommentDeleted;
    public static EventHandler<Ticket> OnTicketCreated;
    public static EventHandler<TicketMessageEventArgs> OnTicketMessage;
    public static EventHandler<Ticket> OnTicketUpdated;
    public static EventHandler OnMoonlightRestart;
}