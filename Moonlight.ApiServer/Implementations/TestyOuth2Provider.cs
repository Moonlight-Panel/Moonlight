using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Interfaces;
using MoonCore.Extended.OAuth2.Models;
using MoonCore.Extensions;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.Shared.Http.Responses.OAuth2;

namespace Moonlight.ApiServer.Implementations;

public class TestyOuth2Provider : IOAuth2Provider
{
    public async Task<Dictionary<string, object>> Sync(IServiceProvider provider, AccessData accessData)
    {
        var logger = provider.GetRequiredService<ILogger<TestyOuth2Provider>>();
        
        try
        {
            var configuration = provider.GetRequiredService<AppConfiguration>();

            using var httpClient = new HttpClient();
            
            httpClient.DefaultRequestHeaders.Add("Authorization", accessData.AccessToken);
            
            var response = await httpClient.GetAsync($"{configuration.PublicUrl}/oauth2/info");
            await response.HandlePossibleApiError();
            var info = await response.ParseAsJson<InfoResponse>();

            var userRepo = provider.GetRequiredService<DatabaseRepository<User>>();
            var user = userRepo.Get().FirstOrDefault(x => x.Email == info.Email);

            if (user == null) // User not found, register a new one
            {
                user = userRepo.Add(new User()
                {
                    Email = info.Email,
                    Username = info.Username
                });
            }
            else if (user.Username != info.Username) // Username updated?
            {
                // Username not used by another user?
                if (!userRepo.Get().Any(x => x.Username == info.Username))
                {
                    // Update username
                    user.Username = info.Username;
                    userRepo.Update(user);
                }
            }

            return new()
            {
                {
                    "userId",
                    user.Id
                }
            };
        }
        catch (Exception e)
        {
            logger.LogCritical("Unable to sync user: {e}", e);
            return new();
        }
    }
}