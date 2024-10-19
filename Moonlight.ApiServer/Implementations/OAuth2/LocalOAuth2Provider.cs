using MoonCore.Extended.Abstractions;
using MoonCore.Extensions;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Interfaces.OAuth2;
using Moonlight.Shared.Http.Responses.OAuth2;

namespace Moonlight.ApiServer.Implementations.OAuth2;

public class LocalOAuth2Provider : IOAuth2Provider
{
    public async Task<User?> Sync(IServiceProvider provider, string accessToken)
    {
        var logger = provider.GetRequiredService<ILogger<LocalOAuth2Provider>>();
        
        try
        {
            var configService = provider.GetRequiredService<ConfigService<AppConfiguration>>();
            var config = configService.Get();

            using var httpClient = new HttpClient();
            
            httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);
            
            var response = await httpClient.GetAsync($"{config.PublicUrl}/oauth2/info");
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

            return user;
        }
        catch (Exception e)
        {
            logger.LogCritical("Unable to sync user: {e}", e);
            return null;
        }
    }
}