using MoonCoreUI.Helpers;
using Moonlight.Core.Interfaces.UI.Index;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Core.UI.Components.Cards;

namespace Moonlight.Core.Implementations.UI.Index;

public class GreetingMessages : IIndexPageComponent
{
    public Task<UiComponent> Get()
    {
        var res = new UiComponent()
        {
            Component = ComponentHelper.FromType<TimeBasedGreetingMessages>(),
            Index = int.MinValue
        };

        return Task.FromResult(res);
    }
}