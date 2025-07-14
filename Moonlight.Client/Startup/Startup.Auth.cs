using Microsoft.Extensions.DependencyInjection;
using MoonCore.Blazor.FlyonUi.Auth;
using MoonCore.Permissions;
using Moonlight.Client.Services;

namespace Moonlight.Client.Startup;

public partial class Startup
{
    private Task RegisterAuthentication()
    {
        WebAssemblyHostBuilder.Services.AddAuthorizationCore();
        WebAssemblyHostBuilder.Services.AddCascadingAuthenticationState();

        WebAssemblyHostBuilder.Services.AddAuthenticationStateManager<RemoteAuthStateManager>();
        
        WebAssemblyHostBuilder.Services.AddAuthorizationPermissions(options =>
        {
            options.ClaimName = "permissions";
            options.Prefix = "permissions:";
        });

        return Task.CompletedTask;
    }
}