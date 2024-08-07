using Microsoft.AspNetCore.Components;

namespace Moonlight.Client.App.Interfaces;

public interface IAppScreen
{
    public int Priority { get; }
    
    public bool ShouldBeShown(IServiceProvider serviceProvider);
    public RenderFragment Render();
}