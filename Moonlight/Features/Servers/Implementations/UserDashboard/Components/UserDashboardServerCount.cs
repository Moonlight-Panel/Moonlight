using MoonCore.Blazor.Helpers;
using Moonlight.Core.Interfaces.UI.User;
using Moonlight.Core.Models.Abstractions;

namespace Moonlight.Features.Servers.Implementations.UserDashboard.Components;

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