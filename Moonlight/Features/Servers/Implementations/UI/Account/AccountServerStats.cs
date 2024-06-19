using MoonCoreUI.Helpers;
using Moonlight.Core.Interfaces.UI.Account;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Features.Servers.UI.Components.Cards;

namespace Moonlight.Features.Servers.Implementations.UI.Account;

public class AccountServerStats : IAccountOverviewComponent
{
    public Task<UiComponent> Get()
    {
        var res = new UiComponent()
        {
            Component = ComponentHelper.FromType<AccountServerStatsComponent>(),
            Index = int.MinValue + 10
        };

        return Task.FromResult(res);
    }
}