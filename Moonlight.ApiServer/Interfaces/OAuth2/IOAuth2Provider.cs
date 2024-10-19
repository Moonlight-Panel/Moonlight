using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Interfaces.OAuth2;

public interface IOAuth2Provider
{
    public Task<User?> Sync(IServiceProvider provider, string accessToken, string refreshToken);
}