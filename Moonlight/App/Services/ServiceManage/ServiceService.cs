using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.ServiceManage;

public class ServiceService // This service is used for managing services and create the connection to the actual logic behind a service type
{
    private readonly IServiceProvider ServiceProvider;
    private readonly Repository<Service> ServiceRepository;

    public ServiceAdminService Admin => ServiceProvider.GetRequiredService<ServiceAdminService>();

    public ServiceService(IServiceProvider serviceProvider, Repository<Service> serviceRepository)
    {
        ServiceProvider = serviceProvider;
        ServiceRepository = serviceRepository;
    }

    public Task<Service[]> Get(User user)
    {
        var result = ServiceRepository
            .Get()
            .Include(x => x.Product)
            .Where(x => x.Owner.Id == user.Id)
            .ToArray();
        
        return Task.FromResult(result);
    }

    public Task<Service[]> GetShared(User user)
    {
        var result = ServiceRepository
            .Get()
            .Include(x => x.Product)
            .Include(x => x.Owner)
            .Where(x => x.Shares.Any(y => y.User.Id == user.Id))
            .ToArray();
        
        return Task.FromResult(result);
    }
}