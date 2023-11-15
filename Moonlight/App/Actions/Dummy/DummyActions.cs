using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Models.Abstractions;
using Moonlight.App.Models.Abstractions.Services;

namespace Moonlight.App.Actions.Dummy;

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