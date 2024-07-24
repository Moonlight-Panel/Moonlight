namespace Moonlight.ApiServer.App.Interfaces;

public interface IAuthenticationProvider
{
    public Task<int> Login(IServiceProvider serviceProvider, string identifier, string password);
    public Task<int> Register(IServiceProvider serviceProvider, string email, string username, string password);
    public Task<DateTime> GetTokenValidTimestamp(IServiceProvider serviceProvider, int userId);
}