using Microsoft.AspNetCore.Components;
using MoonCore.Blazor.Helpers;
using Moonlight.Client.Interfaces;
using Moonlight.Client.Services;
using Moonlight.Client.UI.Screens;

namespace Moonlight.Client.Implementations;

public class AuthenticationUiHandler : IAppLoader, IAppScreen
{
    public int Priority => 0;

    public Task<bool> ShouldRender(IServiceProvider serviceProvider)
    {
        return Task.FromResult<bool>(false);
        var identityService = serviceProvider.GetRequiredService<IdentityService>();
        
        return Task.FromResult(!identityService.IsLoggedIn); // Only show the screen when we are not logged in
    }

    public RenderFragment Render() => ComponentHelper.FromType<AuthenticationScreen>();

    public async Task Load(IServiceProvider serviceProvider)
    {
        return;
        var identityService = serviceProvider.GetRequiredService<IdentityService>();
        await identityService.Check();
    }
}