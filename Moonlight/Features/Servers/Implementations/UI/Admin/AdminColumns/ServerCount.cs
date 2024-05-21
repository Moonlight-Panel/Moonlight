using MoonCoreUI.Helpers;
using Moonlight.Core.Interfaces.Ui.Admin;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Features.Servers.UI.Components.Cards;

namespace Moonlight.Features.Servers.Implementations.UI.Admin.AdminColumns;

public class ServerCount : IAdminDashboardColumn
{
    public Task<UiComponent> Get()
    {
        var res = new UiComponent()
        {
            Component = ComponentHelper.FromType<AdminServersCard>()
        };

        return Task.FromResult(res);
    }
}