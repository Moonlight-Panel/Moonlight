using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Exceptions;
using Moonlight.Core.Database.Entities;


using Moonlight.Features.ServiceManagement.Entities;
using Moonlight.Features.StoreSystem.Entities;

namespace Moonlight.Features.ServiceManagement.Services;

[Singleton]
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

        try
        {
            // Call the action for the logic behind the service type
            await impl.Actions.Create(scope.ServiceProvider, finishedService);
        }
        catch (Exception) // Handle any implementation errors and let the creation fail
        {
            serviceRepo.Delete(finishedService);
            throw;
        }
        
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