using Microsoft.AspNetCore.Components;
using Moonlight.App.Actions.Dummy.Layouts;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Abstractions;

namespace Moonlight.App.Actions.Dummy;

public class DummyServiceImplementation : ServiceImplementation
{
    public override ServiceActions Actions { get; } = new DummyActions();
    public override Type ConfigType { get; } = typeof(DummyConfig);
    
    public override RenderFragment GetAdminLayout()
    {
        return ComponentHelper.FromType(typeof(DummyAdmin));
    }

    public override RenderFragment GetUserLayout()
    {
        return ComponentHelper.FromType(typeof(DummyUser));
    }

    public override ServiceUiPage[] GetUserPages(Service service, User user)
    {
        return Array.Empty<ServiceUiPage>();
    }

    public override ServiceUiPage[] GetAdminPages(Service service, User user)
    {
        return Array.Empty<ServiceUiPage>();
    }
}