using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Models.Abstractions;
using Moonlight.App.Models.Enums;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.ServiceManage;

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
}