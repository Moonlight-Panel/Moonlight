using Microsoft.AspNetCore.Components;
using Moonlight.Client.App.Helpers;
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

        var x = !identityService.IsLoggedIn;

        var logger = serviceProvider.GetRequiredService<ILogger<AuthenticationScreen>>();
        logger.LogWarning("ShouldBeShown: {value}", x);
        
        return x;
    }

    public RenderFragment Render()
    {
        return ComponentHelper.FromType<LoginScreen>();
    }
}