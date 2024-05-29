using MoonCoreUI.Helpers;
using Moonlight.Core.Interfaces.UI.User;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Features.Servers.UI.Components.Cards;

namespace Moonlight.Features.Servers.Implementations.UI.UserDashboard.Components;

public class UserDashboardServerCount : IUserDashboardComponent
{
    public Task<UiComponent> Get()
    {
        var res = new UiComponent()
        {
            Component = ComponentHelper.FromType<Servers.UI.Components.Cards.UserDashboardServerCount>(),
            Index = int.MinValue + 100
        };

        return Task.FromResult(res);
    }
}