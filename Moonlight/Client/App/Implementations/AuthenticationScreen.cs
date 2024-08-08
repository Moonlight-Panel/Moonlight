using Microsoft.AspNetCore.Components;
using MoonCore.Blazor.Helpers;
using Moonlight.Client.App.Interfaces;
using Moonlight.Client.App.Services;
using Moonlight.Client.App.UI.Components.Auth;

namespace Moonlight.Client.App.Implementations;

public class AuthenticationScreen : IAppScreen
{
    public int Priority => 0;

    private bool ShowLogin = false;

    public bool ShouldBeShown(IServiceProvider serviceProvider)
    {
        var identityService = serviceProvider.GetRequiredService<IdentityService>();
        
        if (identityService.IsLoggedIn)
            return false;

        var navigation = serviceProvider.GetRequiredService<NavigationManager>();

        var uri = new Uri(navigation.Uri);

        ShowLogin = uri.LocalPath != "/register";

        return true;
    }

    public RenderFragment Render()
    {
        if(ShowLogin)
            return ComponentHelper.FromType<LoginScreen>();
        else
            return ComponentHelper.FromType<RegisterScreen>();
    }
}