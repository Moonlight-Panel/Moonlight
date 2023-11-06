using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Community;
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
    public static EventHandler<TransactionCreatedEventArgs> OnTransactionCreated;
    public static EventHandler<Post> OnPostCreated;
    public static EventHandler<Post> OnPostUpdated;
    public static EventHandler<Post> OnPostDeleted;
    public static EventHandler<Post> OnPostLiked;
    public static EventHandler<PostComment> OnPostCommentCreated;
    public static EventHandler<PostComment> OnPostCommentDeleted;
    public static EventHandler OnMoonlightRestart;
}