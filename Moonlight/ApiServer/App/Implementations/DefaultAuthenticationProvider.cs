using System.Text.RegularExpressions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using Moonlight.ApiServer.App.Database.Entities;
using Moonlight.ApiServer.App.Exceptions;
using Moonlight.ApiServer.App.Interfaces;

namespace Moonlight.ApiServer.App.Implementations;

public class DefaultAuthenticationProvider : IAuthenticationProvider
{
    public Task<int> Login(IServiceProvider serviceProvider, string identifier, string password, string? twoFactorCode = null)
    {
        identifier = identifier.ToLower();
        
        if (!Regex.IsMatch(identifier, "^.+@.+$"))
            throw new ApiException("You need to provide a valid email address", statusCode: 400);

        if (string.IsNullOrEmpty(password))
            throw new ApiException("You need to provide a password", statusCode: 400);
        
        var userRepo = serviceProvider.GetRequiredService<DatabaseRepository<User>>();
        var user = userRepo
            .Get()
            .FirstOrDefault(x => x.Email == identifier);

        if (user == null)
            throw new ApiException("A user with these credentials could not be found", statusCode: 400);
        
        if(!HashHelper.Verify(password, user.Password))
            throw new ApiException("A user with these credentials could not be found", statusCode: 400);
        
        // TODO: 2FA

        return Task.FromResult(user.Id);
    }

    public Task<int> Register(IServiceProvider serviceProvider, string email, string username, string password)
    {
        email = email.ToLower();
        
        if (!Regex.IsMatch(email, "^.+@.+$"))
            throw new ApiException("You need to provide a valid email address", statusCode: 400);

        if (!Regex.IsMatch(username, "^[a-z][a-z0-9]*$"))
            throw new ApiException(
                "Usernames can only contain lowercase characters and numbers and should not start with a number",
                statusCode: 400);
        
        if (string.IsNullOrEmpty(password))
            throw new ApiException("You need to provide a password", statusCode: 400);

        if (password.Length < 7 || password.Length > 256)
            throw new ApiException("The password needs to be longer than 7 characters and shorter than 256 characters", statusCode: 400);
        
        var userRepo = serviceProvider.GetRequiredService<DatabaseRepository<User>>();

        if (userRepo.Get().Any(x => x.Email == email))
            throw new ApiException("A user with that email address already exists", statusCode: 400);
        
        if (userRepo.Get().Any(x => x.Username == username))
            throw new ApiException("A user with that username already exists", statusCode: 400);

        var user = new User()
        {
            Email = email,
            Username = username,
            Password = HashHelper.Hash(password)
        };

        var finalUser = userRepo.Add(user);

        return Task.FromResult(finalUser.Id);
    }

    public Task<DateTime> GetTokenValidTimestamp(IServiceProvider serviceProvider, int userId)
    {
        var userRepo = serviceProvider.GetRequiredService<DatabaseRepository<User>>();
        var user = userRepo.Get().First(x => x.Id == userId);
        
        return Task.FromResult(user.TokenValidTime);
    }
}