using Microsoft.AspNetCore.Components;
using Moonlight.Client.Interfaces;
using Moonlight.Client.Services;

namespace Moonlight.Client.Implementations;

public class AuthenticationUiHandler : IAppLoader, IAppScreen
{
    public int Priority => 0;

    public Task<bool> ShouldRender(IServiceProvider serviceProvider)
        => Task.FromResult(false);

    public RenderFragment Render() => throw new NotImplementedException();

    public async Task Load(IServiceProvider serviceProvider)
    {
        var identityService = serviceProvider.GetRequiredService<IdentityService>();
        await identityService.Check();
    }
}