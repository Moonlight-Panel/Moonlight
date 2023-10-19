using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Store;

public class TransactionService
{
    private readonly Repository<User> UserRepository;

    public TransactionService(Repository<User> userRepository)
    {
        UserRepository = userRepository;
    }

    public Task Add(User u, double amount, string message)
    {
        var user = UserRepository.Get().First(x => x.Id == u.Id); // Load user with current repo
        
        user.Transactions.Add(new Transaction()
        {
            Text = message,
            Price = amount
        });
        
        UserRepository.Update(user);

        // We divide the call to ensure the transaction can be written to the database 
        
        user.Balance += amount;
        UserRepository.Update(user);
        
        return Task.CompletedTask;
    }
}