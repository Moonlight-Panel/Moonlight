using Microsoft.EntityFrameworkCore;
using Moonlight.Core.Repositories;
using Moonlight.Features.ServiceManagement.Entities;
using Moonlight.Features.ServiceManagement.Entities.Enums;
using Moonlight.Features.ServiceManagement.Models.Abstractions;
using Moonlight.Features.StoreSystem.Entities;

namespace Moonlight.Features.ServiceManagement.Services;

public class ServiceDefinitionService
{
    private readonly Dictionary<ServiceType, ServiceDefinition> ServiceImplementations = new();
    
    private readonly IServiceScopeFactory ServiceScopeFactory;

    public ServiceDefinitionService(IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
    }

    public void Register<T>(ServiceType type) where T : ServiceDefinition
    {
        var impl = Activator.CreateInstance<T>() as ServiceDefinition;

        if (impl == null)
            throw new ArgumentException("The provided type is not an service implementation");
        
        if (ServiceImplementations.ContainsKey(type))
            throw new ArgumentException($"An implementation for {type} has already been registered");
        
        ServiceImplementations.Add(type, impl);
    }

    public ServiceDefinition Get(Service s)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var serviceRepo = scope.ServiceProvider.GetRequiredService<Repository<Service>>();
        
        var service = serviceRepo
            .Get()
            .Include(x => x.Product)
            .First(x => x.Id == s.Id);

        return Get(service.Product);
    }

    public ServiceDefinition Get(Product p) => Get(p.Type);
    
    public ServiceDefinition Get(ServiceType type)
    {
        if (!ServiceImplementations.ContainsKey(type))
            throw new ArgumentException($"No service implementation found for {type}");

        return ServiceImplementations[type];
    }
}