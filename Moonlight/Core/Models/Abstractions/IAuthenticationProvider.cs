using Moonlight.Core.Database.Entities;

namespace Moonlight.Core.Models.Abstractions;

public interface IAuthenticationProvider
{
    public Task<bool> RequiresTwoFactorCode(string email, string password);
    public Task<User?> Authenticate(string email, string password, string twoFactorCode);
    public Task<User?> Register(string username, string email, string password);
    public Task ChangePassword(User user, string password);
    public Task ChangeDetails(User user, string newEmail, string newUsername);
    public Task SetTwoFactorSecret(User user, string secret);
}