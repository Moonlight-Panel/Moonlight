using Microsoft.EntityFrameworkCore;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Exceptions;
using Moonlight.Core.Repositories;
using Moonlight.Features.ServiceManagement.Entities;

namespace Moonlight.Features.ServiceManagement.Services;

public class ServiceService // This service is used for managing services and create the connection to the actual logic behind a service type
{
    private readonly IServiceProvider ServiceProvider;
    private readonly Repository<Service> ServiceRepository;
    private readonly Repository<User> UserRepository;

    public ServiceAdminService Admin => ServiceProvider.GetRequiredService<ServiceAdminService>();
    public ServiceDefinitionService Definition => ServiceProvider.GetRequiredService<ServiceDefinitionService>();
    public ServiceManageService Manage => ServiceProvider.GetRequiredService<ServiceManageService>();

    public ServiceService(IServiceProvider serviceProvider, Repository<Service> serviceRepository, Repository<User> userRepository)
    {
        ServiceProvider = serviceProvider;
        ServiceRepository = serviceRepository;
        UserRepository = userRepository;
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

    public Task AddSharedUser(Service s, string username)
    {
        var userToAdd = UserRepository
            .Get()
            .FirstOrDefault(x => x.Username == username);

        if (userToAdd == null)
            throw new DisplayException("No user found with this username");

        var service = ServiceRepository
            .Get()
            .Include(x => x.Owner)
            .Include(x => x.Shares)
            .ThenInclude(x => x.User)
            .First(x => x.Id == s.Id);

        if (service.Owner.Id == userToAdd.Id)
            throw new DisplayException("The owner cannot be added as a shared user");

        if (service.Shares.Any(x => x.User.Id == userToAdd.Id))
            throw new DisplayException("The user has already access to this service");
        
        service.Shares.Add(new ()
        {
            User = userToAdd
        });
        
        ServiceRepository.Update(service);
        
        return Task.CompletedTask;
    }

    public Task<User[]> GetSharedUsers(Service s)
    {
        var service = ServiceRepository
            .Get()
            .Include(x => x.Shares)
            .ThenInclude(x => x.User)
            .First(x => x.Id == s.Id);

        var result = service.Shares
            .Select(x => x.User)
            .ToArray();

        return Task.FromResult(result);
    }

    public Task RemoveSharedUser(Service s, User user)
    {
        var service = ServiceRepository
            .Get()
            .Include(x => x.Shares)
            .ThenInclude(x => x.User)
            .First(x => x.Id == s.Id);

        var shareToRemove = service.Shares.FirstOrDefault(x => x.User.Id == user.Id);

        if (shareToRemove == null)
            throw new DisplayException("This user does not have access to this service");

        service.Shares.Remove(shareToRemove);
        ServiceRepository.Update(service);
        
        return Task.CompletedTask;
    }
}