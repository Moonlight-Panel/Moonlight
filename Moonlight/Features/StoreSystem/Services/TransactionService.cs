using MoonCore.Abstractions;
using MoonCore.Attributes;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Event;
using Moonlight.Core.Extensions;
using Moonlight.Features.StoreSystem.Entities;

namespace Moonlight.Features.StoreSystem.Services;

[Scoped]
public class TransactionService
{
    private readonly Repository<User> UserRepository;

    public TransactionService(Repository<User> userRepository)
    {
        UserRepository = userRepository;
    }

    public async Task Add(User u, double amount, string message)
    {
        var user = UserRepository.Get().First(x => x.Id == u.Id); // Load user with current repo

        var transaction = new Transaction()
        {
            Text = message,
            Price = amount
        };
        
        user.Transactions.Add(transaction);
        UserRepository.Update(user);

        // We divide the call to ensure the transaction can be written to the database 
        
        user.Balance += amount;
        user.Balance = Math.Round(user.Balance, 2); // To prevent weird numbers
        
        UserRepository.Update(user);

        await Events.OnTransactionCreated.InvokeAsync(new()
        {
            Transaction = transaction,
            User = user
        });
    }
}