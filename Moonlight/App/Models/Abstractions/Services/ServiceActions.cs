namespace Moonlight.App.Models.Abstractions.Services;

public abstract class ServiceActions
{
    public abstract Task Create(IServiceProvider provider, Database.Entities.Store.Service service);
    public abstract Task Update(IServiceProvider provider, Database.Entities.Store.Service service);
    public abstract Task Delete(IServiceProvider provider, Database.Entities.Store.Service service);
}