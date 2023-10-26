using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;

namespace Moonlight.App.Event.Args;

public class TransactionCreatedEventArgs
{
    public Transaction Transaction { get; set; }
    public User User { get; set; }
}