using System.Text.Json;
using MoonCore.Extensions;
using MoonCore.Helpers;
using Moonlight.Client.App.Interfaces;
using Moonlight.Client.App.Services;
using Moonlight.Shared.Http.Resources.Auth;

namespace Moonlight.Client.App.Implementations;

public class AuthenticationStateLoader : IAppLoader
{
    public async Task Load(IServiceProvider serviceProvider)
    {
        var identityService = serviceProvider.GetRequiredService<IdentityService>();
        var httpApiClient = serviceProvider.GetRequiredService<HttpApiClient>();
        var cookieService = serviceProvider.GetRequiredService<CookieService>();

        var token = await cookieService.GetValue("ml-token");
        
        if(!string.IsNullOrEmpty(token))
            identityService.SetToken(token);

        try
        {
            var response = await httpApiClient.GetJson<CheckResponse>("auth/check");
            
            Console.WriteLine(JsonSerializer.Serialize(response));

            identityService.Email = response.Email;
            identityService.Username = response.Username;
            identityService.Permissions = response.Permissions;
            
            identityService.SetLoginState(true);
        }
        catch (Exception)
        {
            identityService.SetLoginState(false);
        }
    }
}