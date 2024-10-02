using Moonlight.Client.UI.Layouts;

namespace Moonlight.Client.Interfaces;

public interface IAppLoader
{
    public int Priority { get; }
    public Task Load(IServiceProvider serviceProvider);
}