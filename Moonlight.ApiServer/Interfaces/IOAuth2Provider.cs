using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Interfaces;

public interface IOAuth2Provider
{
    public Task<string> Start();
    
    public Task<User?> Complete(string code);
}