using Moonlight.Core.Actions.Dummy.Layouts;
using Moonlight.Core.Actions.Dummy.Pages;
using Moonlight.Core.Helpers;
using Moonlight.Features.ServiceManagement.Models.Abstractions;

namespace Moonlight.Core.Actions.Dummy;

public class DummyServiceDefinition : ServiceDefinition
{
    public override ServiceActions Actions => new DummyActions();
    public override Type ConfigType => typeof(DummyConfig);
    public override async Task BuildUserView(ServiceViewContext context)
    {
        context.Layout = typeof(DummyUser);

        await context.AddPage<DummyPage>("Demo", "/demo");
    }

    public override Task BuildAdminView(ServiceViewContext context)
    {
        context.Layout = typeof(DummyAdmin);
        
        return Task.CompletedTask;
    }
}