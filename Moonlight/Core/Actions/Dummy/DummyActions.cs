using Moonlight.Core.Database.Entities.Store;
using Moonlight.Core.Models.Abstractions.Services;

namespace Moonlight.Core.Actions.Dummy;

public class DummyActions : ServiceActions
{
    public override Task Create(IServiceProvider provider, Service service)
    {
        return Task.CompletedTask;
    }

    public override Task Update(IServiceProvider provider, Service service)
    {
        return Task.CompletedTask;
    }

    public override Task Delete(IServiceProvider provider, Service service)
    {
        return Task.CompletedTask;
    }
}