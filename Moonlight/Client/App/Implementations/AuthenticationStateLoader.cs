using Moonlight.Client.App.Interfaces;
using Moonlight.Client.App.Services;

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
            await identityService.Http.GetStringAsync("auth/check");
            identityService.SetLoginState(true);
        }
        catch (Exception)
        {
            identityService.SetLoginState(false);
        }
    }
}