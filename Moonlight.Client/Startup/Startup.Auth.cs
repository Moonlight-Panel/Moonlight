using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MoonCore.Blazor.FlyonUi.Exceptions;
using MoonCore.Permissions;
using Moonlight.Client.Implementations;
using Moonlight.Client.Services;

namespace Moonlight.Client.Startup;

public partial class Startup
{
    private Task RegisterAuthenticationAsync()
    {
        WebAssemblyHostBuilder.Services.AddAuthorizationCore();
        WebAssemblyHostBuilder.Services.AddCascadingAuthenticationState();

        WebAssemblyHostBuilder.Services.AddScoped<AuthenticationStateProvider, RemoteAuthStateProvider>();
        WebAssemblyHostBuilder.Services.AddScoped<IGlobalErrorFilter, UnauthenticatedErrorFilter>();
        
        WebAssemblyHostBuilder.Services.AddAuthorizationPermissions(options =>
        {
            options.ClaimName = "Permissions";
            options.Prefix = "permissions:";
        });

        return Task.CompletedTask;
    }
}