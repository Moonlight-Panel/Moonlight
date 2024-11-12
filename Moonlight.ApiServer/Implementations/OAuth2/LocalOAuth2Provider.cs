using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Extended.OAuth2.LocalProvider;
using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Implementations.OAuth2;

public class LocalOAuth2Provider : ILocalProviderImplementation<User>
{
    private readonly DatabaseRepository<User> UserRepository;

    public LocalOAuth2Provider(DatabaseRepository<User> userRepository)
    {
        UserRepository = userRepository;
    }
    
    public Task SaveChanges(User model)
    {
        UserRepository.Update(model);
        return Task.CompletedTask;
    }

    public Task<User?> LoadById(int id)
    {
        var res = UserRepository.Get().FirstOrDefault(x => x.Id == id);

        return Task.FromResult(res);
    }

    public Task<User> Login(string email, string password)
    {
        var user = UserRepository.Get().FirstOrDefault(x => x.Email == email);

        if (user == null)
            throw new HttpApiException("Invalid email or password", 400);
        
        if(!HashHelper.Verify(password, user.Password))
            throw new HttpApiException("Invalid email or password", 400);
        
        return Task.FromResult(user);
    }

    public Task<User> Register(string username, string email, string password)
    {
        if (UserRepository.Get().Any(x => x.Username == username))
            throw new HttpApiException("A user with that username already exists", 400);

        if (UserRepository.Get().Any(x => x.Email == email))
            throw new HttpApiException("A user with that email address already exists", 400);

        var user = new User()
        {
            Username = username,
            Email = email,
            Password = HashHelper.Hash(password)
        };

        var finalUser = UserRepository.Add(user);
        
        return Task.FromResult(finalUser);
    }
}