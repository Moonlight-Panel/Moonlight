using Moonlight.Core.Models.Abstractions;

namespace Moonlight.Core.Interfaces.UI.Account;

public interface IAccountOverviewComponent
{
    public Task<UiComponent> Get();
}