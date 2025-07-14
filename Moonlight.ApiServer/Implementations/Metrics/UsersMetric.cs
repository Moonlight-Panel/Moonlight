using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MoonCore.Extended.Abstractions;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Interfaces;

namespace Moonlight.ApiServer.Implementations.Metrics;

public class UsersMetric : IMetric
{
    private Gauge<int> Users;
    
    public Task Initialize(Meter meter)
    {
        Users = meter.CreateGauge<int>("moonlight_users");
        
        return Task.CompletedTask;
    }

    public async Task Run(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var usersRepo = provider.GetRequiredService<DatabaseRepository<User>>();
        var count = await usersRepo.Get().CountAsync(cancellationToken: cancellationToken);
        
        Users.Record(count);
    }
}