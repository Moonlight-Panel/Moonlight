using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Attributes;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Core.Models.Enums;

using Moonlight.Features.ServiceManagement.Entities;

namespace Moonlight.Features.ServiceManagement.Services;

[Singleton]
public class ServiceManageService
{
    private readonly IServiceScopeFactory ServiceScopeFactory;

    public ServiceManageService(IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
    }

    public Task<bool> CheckAccess(Service s, User user)
    {
        var permissionStorage = new PermissionStorage(user.Permissions);
        
        // Is admin?
        if(permissionStorage[Permission.AdminServices])
            return Task.FromResult(true);
        
        using var scope = ServiceScopeFactory.CreateScope();
        var serviceRepo = scope.ServiceProvider.GetRequiredService<Repository<Service>>();

        var service = serviceRepo
            .Get()
            .Include(x => x.Owner)
            .Include(x => x.Shares)
            .ThenInclude(x => x.User)
            .First(x => x.Id == s.Id);
        
        // Is owner?
        if(service.Owner.Id == user.Id)
            return Task.FromResult(true);
        
        // Is shared user
        if(service.Shares.Any(x => x.User.Id == user.Id))
            return Task.FromResult(true);

        // No match
        return Task.FromResult(false);
    }

    public Task<bool> NeedsRenewal(Service s)
    {
        // We fetch the service in a new scope wo ensure that we are not caching
        using var scope = ServiceScopeFactory.CreateScope();
        var serviceRepo = scope.ServiceProvider.GetRequiredService<Repository<Service>>();

        var service = serviceRepo
            .Get()
            .First(x => x.Id == s.Id);
        
        return Task.FromResult(DateTime.UtcNow > service.RenewAt);
    }
}