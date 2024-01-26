using Moonlight.Features.ServiceManagement.Entities;

namespace Moonlight.Features.ServiceManagement.Models.Abstractions;

public abstract class ServiceActions
{
    public abstract Task Create(IServiceProvider provider, Service service);
    public abstract Task Update(IServiceProvider provider, Service service);
    public abstract Task Delete(IServiceProvider provider, Service service);
}