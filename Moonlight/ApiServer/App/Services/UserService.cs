using MoonCore.Attributes;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Services;
using Moonlight.ApiServer.App.Configuration;
using Moonlight.ApiServer.App.Database.Entities;
using Moonlight.ApiServer.App.Exceptions;
using Moonlight.ApiServer.App.Interfaces;

namespace Moonlight.ApiServer.App.Services;

[Scoped]
public class UserService
{
    private readonly IAuthenticationProvider Provider;
    private readonly IServiceProvider ServiceProvider;
    private readonly DatabaseRepository<User> UserRepository;
    private readonly ConfigService<AppConfiguration> ConfigService;
    private readonly JwtHelper JwtHelper;

    public UserService(
        IAuthenticationProvider provider,
        IServiceProvider serviceProvider,
        ConfigService<AppConfiguration> configService,
        DatabaseRepository<User> userRepository,
        JwtHelper jwtHelper)
    {
        Provider = provider;
        ServiceProvider = serviceProvider;
        ConfigService = configService;
        UserRepository = userRepository;
        JwtHelper = jwtHelper;
    }

    public async Task<string> Register(string username, string email, string password)
    {
        var userId = await Provider.Register(ServiceProvider, email, username, password);

        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Id == userId);

        if(user == null)
            throw new ApiException("Register failed. Authentication provider did not register the user", statusCode: 503);

        return await GenerateToken(user);
    }
    
    public async Task<string> Login(string identifier, string password, string? twoFactorCode)
    {
        var userId = await Provider.Login(ServiceProvider, identifier, password, twoFactorCode);

        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Id == userId);

        if(user == null)
            throw new ApiException("Register failed. Authentication provider did not authenticate the user", statusCode: 503);

        return await GenerateToken(user);
    }

    public async Task<string> GenerateToken(User user)
    {
        var config = ConfigService.Get();
        
        var jwt = await JwtHelper.Create(config.Security.Token, data =>
        {
            data.Add("UserId", user.Id.ToString());
        }, "userLogin", TimeSpan.FromDays(config.Authentication.TokenDuration));

        return jwt;
    }
}