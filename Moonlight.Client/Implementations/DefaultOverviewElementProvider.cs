using Moonlight.Client.Interfaces;
using Moonlight.Client.UI.Components;

namespace Moonlight.Client.Implementations;

public class DefaultOverviewElementProvider : IOverviewElementProvider
{
    public void ModifyOverview(List<Type> overviewComponents)
    {
        overviewComponents.Add(typeof(WelcomeOverviewElement));
    }
}