using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Extended.OAuth2.ApiServer;
using MoonCore.Helpers;
using MoonCore.PluginFramework.Services;
using MoonCore.Services;
using Moonlight.ApiServer.Attributes;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Helpers.Authentication;
using Moonlight.ApiServer.Interfaces.Auth;
using Moonlight.ApiServer.Interfaces.OAuth2;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Requests.Auth;
using Moonlight.Shared.Http.Responses.Auth;

namespace Moonlight.ApiServer.Http.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : Controller
{
    private readonly OAuth2Service OAuth2Service;
    private readonly TokenHelper TokenHelper;
    private readonly ConfigService<AppConfiguration> ConfigService;
    private readonly DatabaseRepository<User> UserRepository;
    private readonly ImplementationService ImplementationService;

    public AuthController(
        OAuth2Service oAuth2Service,
        TokenHelper tokenHelper,
        DatabaseRepository<User> userRepository,
        ConfigService<AppConfiguration> configService,
        ImplementationService implementationService
    )
    {
        OAuth2Service = oAuth2Service;
        TokenHelper = tokenHelper;
        UserRepository = userRepository;
        ConfigService = configService;
        ImplementationService = implementationService;
    }

    [HttpGet]
    public async Task<OAuth2StartResponse> Start()
    {
        var data = await OAuth2Service.StartAuthorizing();

        return Mapper.Map<OAuth2StartResponse>(data);
    }

    [HttpPost]
    public async Task<OAuth2HandleResponse> Handle([FromBody] OAuth2HandleRequest request)
    {
        var accessData = await OAuth2Service.RequestAccess(request.Code);

        // Find oauth2 provider
        var provider = ImplementationService.Get<IOAuth2Provider>().FirstOrDefault();

        if (provider == null)
            throw new HttpApiException("No oauth2 provider has been registered", 500);

        // Sync user from oauth2 provider
        var user = await provider.Sync(HttpContext.RequestServices, accessData.AccessToken);

        if (user == null)
            throw new HttpApiException("The oauth2 provider was unable to authenticate you", 401);
        
        // Allow plugins to intercept access calls
        var interceptors = ImplementationService.Get<IAuthInterceptor>();

        if (interceptors.Any(interceptor => !interceptor.AllowAccess(user, HttpContext.RequestServices)))
            throw new HttpApiException("Unable to get access token", 401);

        // Save oauth2 refresh and access tokens for later use (re-authentication etc.).
        // Fetch user model in current db context, just in case the oauth2 provider
        // uses a different db context or smth

        var userModel = UserRepository
            .Get()
            .First(x => x.Id == user.Id);

        userModel.AccessToken = accessData.AccessToken;
        userModel.RefreshToken = accessData.RefreshToken;
        userModel.RefreshTimestamp = DateTime.UtcNow.AddSeconds(accessData.ExpiresIn);

        UserRepository.Update(userModel);

        // Generate local token-pair for the authentication
        // between client and the api server

        var authConfig = ConfigService.Get().Authentication;

        var tokenPair = await TokenHelper.GeneratePair(
            authConfig.AccessSecret,
            authConfig.AccessSecret,
            data => { data.Add("userId", user.Id.ToString()); },
            authConfig.AccessDuration,
            authConfig.RefreshDuration
        );

        // Authentication finished. Return data to client

        return new OAuth2HandleResponse()
        {
            AccessToken = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(authConfig.AccessDuration)
        };
    }

    [HttpPost("refresh")]
    public async Task<RefreshResponse> Refresh([FromBody] RefreshRequest request)
    {
        var authConfig = ConfigService.Get().Authentication;

        var tokenPair = await TokenHelper.RefreshPair(
            request.RefreshToken,
            authConfig.AccessSecret,
            authConfig.RefreshSecret,
            (refreshData, newData)
                => ProcessRefreshData(refreshData, newData, HttpContext.RequestServices),
            authConfig.AccessDuration,
            authConfig.RefreshDuration
        );

        // Handle refresh error
        if (!tokenPair.HasValue)
            throw new HttpApiException("Unable to refresh token", 401);

        // Return data
        return new RefreshResponse()
        {
            AccessToken = tokenPair.Value.AccessToken,
            RefreshToken = tokenPair.Value.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(authConfig.AccessDuration)
        };
    }

    private bool ProcessRefreshData(Dictionary<string, string> refreshTokenData, Dictionary<string, string> newData, IServiceProvider serviceProvider)
    {
        // Find oauth2 provider
        var provider = ImplementationService.Get<IOAuth2Provider>().FirstOrDefault();

        if (provider == null)
            throw new HttpApiException("No oauth2 provider has been registered", 500);
        
        // Check if the userId is present in the refresh token
        if (!refreshTokenData.TryGetValue("userId", out var userIdStr) || !int.TryParse(userIdStr, out var userId))
            return false;

        // Load user from database if existent
        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Id == userId);

        if (user == null)
            return false;

        // Allow plugins to intercept the refresh call
        var interceptors = ImplementationService.Get<IAuthInterceptor>();

        if (interceptors.Any(interceptor => !interceptor.AllowRefresh(user, serviceProvider)))
            return false;

        // Check if it's time to resync with the oauth2 provider
        if (false && DateTime.UtcNow >= user.RefreshTimestamp)
        {
            // It's time to refresh the access to the external oauth2 provider
            var refreshData = OAuth2Service.RefreshAccess(user.RefreshToken).Result;
            
            // Sync user with oauth2 provider
            var syncedUser = provider.Sync(serviceProvider, refreshData.AccessToken).Result;

            if (syncedUser == null) // User sync has failed. No refresh allowed
                return false;
            
            // Save oauth2 refresh and access tokens for later use (re-authentication etc.).
            // Fetch user model in current db context, just in case the oauth2 provider
            // uses a different db context or smth

            var userModel = UserRepository
                .Get()
                .First(x => x.Id == syncedUser.Id);

            userModel.AccessToken = refreshData.AccessToken;
            userModel.RefreshToken = refreshData.RefreshToken;
            userModel.RefreshTimestamp = DateTime.UtcNow.AddSeconds(refreshData.ExpiresIn);

            UserRepository.Update(userModel);
        }
        
        // All checks have passed, allow refresh
        newData.Add("userId", user.Id.ToString());
        return true;
    }

    [HttpGet("check")]
    [RequirePermission("meta.authenticated")]
    public Task<CheckResponse> Check()
    {
        var perm = HttpContext.User as PermClaimsPrinciple;
        var user = perm!.CurrentModel;

        var response = new CheckResponse()
        {
            Email = user.Email,
            Username = user.Username,
            Permissions = perm.Permissions
        };

        return Task.FromResult(response);
    }
}