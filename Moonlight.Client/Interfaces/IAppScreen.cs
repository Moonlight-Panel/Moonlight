using Microsoft.AspNetCore.Components;

namespace Moonlight.Client.Interfaces;

public interface IAppScreen
{
    public int Priority { get; }
    public Task<bool> ShouldRender(IServiceProvider serviceProvider);
    public RenderFragment Render();
}