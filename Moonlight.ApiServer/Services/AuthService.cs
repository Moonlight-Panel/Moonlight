using MoonCore.Attributes;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Services;

[Scoped]
public class AuthService
{
    private readonly DatabaseRepository<User> UserRepository;
    private readonly ConfigService<AppConfiguration> ConfigService;

    public AuthService(
        DatabaseRepository<User> userRepository,
        ConfigService<AppConfiguration> configService)
    {
        UserRepository = userRepository;
        ConfigService = configService;
    }

    public Task<User> Register(string username, string email, string password)
    {
        // Reformat values
        username = username.ToLower().Trim();
        email = email.ToLower().Trim();
        
        // Check for users with the same values
        if (UserRepository.Get().Any(x => x.Username == username))
            throw new HttpApiException("A user with that username already exists", 400);
        
        if (UserRepository.Get().Any(x => x.Email == email))
            throw new HttpApiException("A user with that email address already exists", 400);
        
        // Build model and add it to the database
        var user = new User()
        {
            Username = username,
            Email = email,
            Password = HashHelper.Hash(password),
            PermissionsJson = "[]",
            TokenValidTimestamp = DateTime.UtcNow
        };

        UserRepository.Add(user);
        
        return Task.FromResult(user);
    }

    public Task<User> Login(string email, string password)
    {
        // Reformat values
        email = email.ToLower().Trim();
        
        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Email == email);

        if (user == null)
            throw new HttpApiException("Invalid email or password", 400);

        if(!HashHelper.Verify(password, user.Password))
            throw new HttpApiException("Invalid email or password", 400);
        
        return Task.FromResult(user);
    }
}