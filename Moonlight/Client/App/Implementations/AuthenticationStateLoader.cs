using MoonCore.Extensions;
using Moonlight.Client.App.Interfaces;
using Moonlight.Client.App.Services;
using Moonlight.Shared.Http.Resources.Auth;

namespace Moonlight.Client.App.Implementations;

public class AuthenticationStateLoader : IAppLoader
{
    public async Task Load(IServiceProvider serviceProvider)
    {
        var identityService = serviceProvider.GetRequiredService<IdentityService>();
        var cookieService = serviceProvider.GetRequiredService<CookieService>();

        var token = await cookieService.GetValue("ml-token");
        
        if(!string.IsNullOrEmpty(token))
            identityService.SetToken(token);

        try
        {
            var response = await identityService.Http.GetAsync("auth/check");

            await response.HandlePossibleApiError();

            var checkResponse = await response.ParseAsJson<CheckResponse>();

            identityService.Email = checkResponse.Email;
            identityService.Username = checkResponse.Username;
            
            identityService.SetLoginState(true);
        }
        catch (Exception)
        {
            identityService.SetLoginState(false);
        }
    }
}