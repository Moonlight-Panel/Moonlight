using System.Text.RegularExpressions;
using MoonCore.Extended.Abstractions;
using Moonlight.ApiServer.App.Database.Entities;
using Moonlight.ApiServer.App.Exceptions;
using Moonlight.ApiServer.App.Interfaces;

namespace Moonlight.ApiServer.App.Implementations;

public class DefaultAuthenticationProvider : IAuthenticationProvider
{
    public Task<int> Login(IServiceProvider serviceProvider, string identifier, string password)
    {
        identifier = identifier.ToLower();
        
        if (!Regex.IsMatch(identifier, "^.+@.+$"))
            throw new ApiException("You need to provide a valid email address", statusCode: 400);

        if (!string.IsNullOrEmpty(password))
            throw new ApiException("You need to provide a password", statusCode: 400);
        
        var userRepo = serviceProvider.GetRequiredService<DatabaseRepository<User>>();
        var user = userRepo
            .Get()
            .FirstOrDefault(x => x.Email == identifier);

        if (user == null)
            throw new ApiException("");
    }

    public Task<int> Register(IServiceProvider serviceProvider, string email, string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<DateTime> GetTokenValidTimestamp(IServiceProvider serviceProvider, int userId)
    {
        throw new NotImplementedException();
    }
}