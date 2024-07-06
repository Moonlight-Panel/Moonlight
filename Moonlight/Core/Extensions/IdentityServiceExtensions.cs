using MoonCore.Abstractions;
using MoonCore.Services;
using Moonlight.Core.Database.Entities;

namespace Moonlight.Core.Extensions;

public static class IdentityServiceExtensions
{
    public static User GetUser(this IdentityService identityService)
    {
        return identityService.Storage.Get<User>();
    }

    public static Task<bool> HasFlag(this IdentityService identityService, string flag)
    {
        if (!identityService.IsAuthenticated)
            return Task.FromResult(false);

        var result = identityService.GetUser().Flags.Split(";").Contains(flag);
        return Task.FromResult(result);
    }

    public static Task SetFlag(this IdentityService identityService, string flag, bool toggle)
    {
        if (!identityService.IsAuthenticated)
            return Task.CompletedTask;

        var user = identityService.GetUser();
        
        // Rebuild flags
        var flags = user.Flags.Split(";").ToList();

        if (toggle)
        {
            if(!flags.Contains(flag))
                flags.Add(flag);
        }
        else
        {
            if (flags.Contains(flag))
                flags.Remove(flag);
        }

        user.Flags = string.Join(';', flags);

        // Save changes
        var serviceProvider = identityService.Storage.Get<IServiceProvider>();
        var userRepo = serviceProvider.GetRequiredService<Repository<User>>();
        
        userRepo.Update(user);
        
        return Task.CompletedTask;
    }
}