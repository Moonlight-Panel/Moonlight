using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Exceptions;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.ServiceManage;

public class ServiceAdminService
{
    private readonly IServiceScopeFactory ServiceScopeFactory;
    private readonly ServiceDefinitionService ServiceDefinitionService;

    public ServiceAdminService(IServiceScopeFactory serviceScopeFactory, ServiceDefinitionService serviceDefinitionService)
    {
        ServiceScopeFactory = serviceScopeFactory;
        ServiceDefinitionService = serviceDefinitionService;
    }

    public async Task<Service> Create(User u, Product p, Action<Service>? modifyService = null)
    {
        var impl = ServiceDefinitionService.Get(p);
        
        // Load models in new scope
        using var scope = ServiceScopeFactory.CreateScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();
        var productRepo = scope.ServiceProvider.GetRequiredService<Repository<Product>>();
        var serviceRepo = scope.ServiceProvider.GetRequiredService<Repository<Service>>();
        
        var user = userRepo.Get().First(x => x.Id == u.Id);
        var product = productRepo.Get().First(x => x.Id == p.Id);

        // Create database model
        var service = new Service()
        {
            Product = product,
            Owner = user,
            Suspended = false,
            CreatedAt = DateTime.UtcNow
        };
        
        // Allow further modifications
        if(modifyService != null)
            modifyService.Invoke(service);

        // Add new service in database
        var finishedService = serviceRepo.Add(service);

        // Call the action for the logic behind the service type
        await impl.Actions.Create(scope.ServiceProvider, finishedService);

        return finishedService;
    }

    public async Task Delete(Service s)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var serviceRepo = scope.ServiceProvider.GetRequiredService<Repository<Service>>();
        var serviceShareRepo = scope.ServiceProvider.GetRequiredService<Repository<ServiceShare>>();

        var service = serviceRepo
            .Get()
            .Include(x => x.Shares)
            .FirstOrDefault(x => x.Id == s.Id);

        if (service == null)
            throw new DisplayException("Service does not exist anymore");

        var impl = ServiceDefinitionService.Get(service);
        
        await impl.Actions.Delete(scope.ServiceProvider, service);

        foreach (var share in service.Shares.ToArray())
        {
            serviceShareRepo.Delete(share);
        }
        
        serviceRepo.Delete(service);
    }
}