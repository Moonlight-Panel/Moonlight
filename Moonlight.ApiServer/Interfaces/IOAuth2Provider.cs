using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Interfaces;

public interface IOAuth2Provider
{
    public Task<User?> Sync(string code);
}