using Moonlight.App.Actions.Dummy.Layouts;
using Moonlight.App.Actions.Dummy.Pages;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Abstractions.Services;

namespace Moonlight.App.Actions.Dummy;

public class DummyServiceDefinition : ServiceDefinition
{
    public override ServiceActions Actions => new DummyActions();
    public override Type ConfigType => typeof(DummyConfig);
    public override async Task BuildUserView(ServiceViewContext context)
    {
        context.Layout = ComponentHelper.FromType<DummyUser>();

        await context.AddPage<DummyPage>("Demo", "/demo");
    }

    public override Task BuildAdminView(ServiceViewContext context)
    {
        context.Layout = ComponentHelper.FromType<DummyAdmin>();
        
        return Task.CompletedTask;
    }
}