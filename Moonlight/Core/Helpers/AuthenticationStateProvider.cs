using MoonCore.Abstractions;
using MoonCore.Helpers;
using Moonlight.Core.Database.Entities;

namespace Moonlight.Core.Helpers;

public class AuthenticationStateProvider : MoonCore.Abstractions.AuthenticationStateProvider
{
    public override Task<bool> IsValidIdentifier(IServiceProvider provider, string identifier)
    {
        if(!int.TryParse(identifier, out int searchId))
            return Task.FromResult(false);
        
        var userRepo = provider.GetRequiredService<Repository<User>>();
        var result = userRepo.Get().Any(x => x.Id == searchId);
        
        return Task.FromResult(result);
    }

    public override Task LoadFromIdentifier(IServiceProvider provider, string identifier, DynamicStorage storage)
    {
        if(!int.TryParse(identifier, out int searchId))
            return Task.CompletedTask;
        
        var userRepo = provider.GetRequiredService<Repository<User>>();
        var user = userRepo.Get().FirstOrDefault(x => x.Id == searchId);
        
        if(user == null)
            return Task.CompletedTask;
        
        storage.Set("User", user);
        storage.Set("ServiceProvider", provider);
        
        return Task.CompletedTask;
    }

    public override Task<DateTime> DetermineTokenValidTimestamp(IServiceProvider provider, string identifier)
    {
        if(!int.TryParse(identifier, out int searchId))
            return Task.FromResult(DateTime.MaxValue);
        
        var userRepo = provider.GetRequiredService<Repository<User>>();
        var user = userRepo.Get().FirstOrDefault(x => x.Id == searchId);
        
        if(user == null)
            return Task.FromResult(DateTime.MaxValue);

        return Task.FromResult(user.TokenValidTimestamp);
    }
}