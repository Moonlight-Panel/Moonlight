using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MoonCore.Blazor.FlyonUi.Exceptions;
using MoonCore.Permissions;
using Moonlight.Client.Implementations;
using Moonlight.Client.Services;

namespace Moonlight.Client.Startup;

public static partial class Startup
{
    private static void AddAuth(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddScoped<AuthenticationStateProvider, RemoteAuthStateProvider>();
        builder.Services.AddScoped<IGlobalErrorFilter, UnauthenticatedErrorFilter>();
        
        builder.Services.AddAuthorizationPermissions(options =>
        {
            options.ClaimName = "Permissions";
            options.Prefix = "permissions:";
        });
    }
}