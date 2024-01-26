using Moonlight.Core.Database.Entities;
using Moonlight.Features.StoreSystem.Entities;

namespace Moonlight.Core.Event.Args;

public class TransactionCreatedEventArgs
{
    public Transaction Transaction { get; set; }
    public User User { get; set; }
}