namespace Moonlight.Client.App.Interfaces;

public interface IAppLoader
{
    public Task Load(IServiceProvider serviceProvider);
}