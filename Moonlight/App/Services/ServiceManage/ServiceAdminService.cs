using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Database.Enums;
using Moonlight.App.Exceptions;
using Moonlight.App.Models.Abstractions;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.ServiceManage;

public class ServiceAdminService
{
    public readonly Dictionary<ServiceType, ServiceActions> Actions = new();
    private readonly IServiceScopeFactory ServiceScopeFactory;

    public ServiceAdminService(IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
    }

    public async Task<Service> Create(User u, Product p, Action<Service>? modifyService = null)
    {
        if (!Actions.ContainsKey(p.Type))
            throw new DisplayException($"The product type {p.Type} is not registered");
        
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
        var actions = Actions[product.Type];
        await actions.Create(scope.ServiceProvider, finishedService);

        return finishedService;
    }

    public Task RegisterAction(ServiceType type, ServiceActions actions) // Use this function to register service types
    {
        Actions.Add(type, actions);
        return Task.CompletedTask;
    }
}