using MoonCoreUI.Helpers;
using Moonlight.Core.Interfaces.UI.Account;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Core.UI.Components.Cards;

namespace Moonlight.Core.Implementations.UI.Account;

public class AccountProfileOverview : IAccountOverviewComponent
{
    public Task<UiComponent> Get()
    {
        var res = new UiComponent()
        {
            Component = ComponentHelper.FromType<Core.UI.Components.Cards.AccountProfileOverview>(),
            Index = int.MinValue
        };

        return Task.FromResult(res);
    }
}