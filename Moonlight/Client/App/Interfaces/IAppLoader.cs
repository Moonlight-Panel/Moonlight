namespace Moonlight.Client.App.Interfaces;

public interface IAppLoader
{
    public int Priority { get; }
    public Task Load(IServiceProvider serviceProvider);
}