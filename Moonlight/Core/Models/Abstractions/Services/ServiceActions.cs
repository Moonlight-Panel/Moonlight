using Moonlight.Core.Database.Entities.Store;

namespace Moonlight.Core.Models.Abstractions.Services;

public abstract class ServiceActions
{
    public abstract Task Create(IServiceProvider provider, Service service);
    public abstract Task Update(IServiceProvider provider, Service service);
    public abstract Task Delete(IServiceProvider provider, Service service);
}