using MoonCore.Blazor.Helpers;
using Moonlight.Core.Interfaces.Ui.Admin;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Core.UI.Components.Cards;

namespace Moonlight.Core.Implementations.AdminDashboard;

public class UserCount : IAdminDashboardColumn
{
    public Task<UiComponent> Get()
    {
        var res = new UiComponent()
        {
            Component = ComponentHelper.FromType<AdminUserCard>(),
            Index = int.MinValue
        };

        return Task.FromResult(res);
    }
}