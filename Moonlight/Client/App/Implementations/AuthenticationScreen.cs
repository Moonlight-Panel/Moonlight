using Microsoft.AspNetCore.Components;
using MoonCore.Blazor.Helpers;
using Moonlight.Client.App.Interfaces;
using Moonlight.Client.App.Services;
using Moonlight.Client.App.UI.Components.Auth;

namespace Moonlight.Client.App.Implementations;

public class AuthenticationScreen : IAppScreen
{
    public int Priority => 0;

    public bool ShouldBeShown(IServiceProvider serviceProvider)
    {
        var identityService = serviceProvider.GetRequiredService<IdentityService>();
        
        if (identityService.IsLoggedIn)
            return false;

        return true;
    }

    public RenderFragment Render() => ComponentHelper.FromType<AuthScreen>();
}